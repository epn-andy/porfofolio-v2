using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Data;
using PortfolioAPI.Models;

namespace PortfolioAPI;

/// <summary>
/// Run once via: dotnet run --seed
/// Seeds the initial admin user. Set AdminSettings in appsettings.json first.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var email = config["AdminSettings:Email"] ?? "admin@example.com";
        var rawPassword = config["AdminSettings:PlainPassword"];

        if (string.IsNullOrEmpty(rawPassword))
        {
            Console.WriteLine("AdminSettings:PlainPassword not set — skipping admin seed.");
            return;
        }

        if (!await db.Admins.AnyAsync(a => a.Email == email))
        {
            db.Admins.Add(new Admin
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword)
            });
            await db.SaveChangesAsync();
            Console.WriteLine($"Admin seeded: {email}");
        }
        else
        {
            Console.WriteLine("Admin already exists — skipping seed.");
        }
    }
}
