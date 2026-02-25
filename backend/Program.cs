using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioAPI.Data;
using PortfolioAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Authentication (JWT via HttpOnly Cookie) ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
        // Read JWT from HttpOnly cookie
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["auth_token"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// --- CORS ---
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// --- Rate Limiting ---
builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("public", limiterOpt =>
    {
        limiterOpt.PermitLimit = 60;
        limiterOpt.Window = TimeSpan.FromMinutes(1);
        limiterOpt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOpt.QueueLimit = 0;
    });
    opt.AddFixedWindowLimiter("login", limiterOpt =>
    {
        limiterOpt.PermitLimit = 10;
        limiterOpt.Window = TimeSpan.FromMinutes(1);
        limiterOpt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOpt.QueueLimit = 0;
    });
    opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddControllers();

var app = builder.Build();

// --- Middleware Pipeline ---
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Run seeder: dotnet run --seed
if (args.Contains("--seed"))
{
    await PortfolioAPI.DbSeeder.SeedAsync(app.Services, app.Configuration);
    return;
}

app.Run();
