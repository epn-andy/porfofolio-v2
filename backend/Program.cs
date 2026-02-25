using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioAPI.Data;
using PortfolioAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Run DB tasks without starting the web server ---
if (args.Contains("--migrate") || args.Contains("--seed"))
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    // Suppress Kestrel binding entirely
    builder.WebHost.UseUrls("http://127.0.0.1:0");

    var cliApp = builder.Build();

    if (args.Contains("--migrate"))
    {
        Console.WriteLine("Running schema migration...");
        using var scope = cliApp.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create all tables for a brand-new database
        await db.Database.EnsureCreatedAsync();

        // Idempotent additions for existing databases
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""CvFiles"" (
                ""Id""          serial PRIMARY KEY,
                ""FileName""    text NOT NULL,
                ""ContentType"" text NOT NULL,
                ""Data""        bytea NOT NULL,
                ""UploadedAt""  timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'utc')
            );
        ");

        Console.WriteLine("Migration complete.");
    }

    if (args.Contains("--seed"))
    {
        Console.WriteLine("Seeding admin user...");
        await PortfolioAPI.DbSeeder.SeedAsync(cliApp.Services, cliApp.Configuration);
        Console.WriteLine("Seed complete.");
    }

    return;
}
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
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opt =>
{
    opt.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

var app = builder.Build();

// --- Middleware Pipeline ---
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
