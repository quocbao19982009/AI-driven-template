using Backend.Features._FeatureTemplate;
using Backend.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, IConfiguration configuration)
    {
        logger.LogInformation("DataSeeder: Running development seed...");

        await SeedAdminUserAsync(context, logger, configuration);
        await SeedFeaturesAsync(context, logger);
        // TODO: Add new seed methods here as you add new features
        // await SeedProductsAsync(context, logger);
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext context, ILogger logger, IConfiguration configuration)
    {
        const string adminEmail = "admin@example.com";

        if (await context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            logger.LogInformation("DataSeeder: Admin user already exists — skipping");
            return;
        }

        var rawPassword = configuration["Seed:AdminPassword"] ?? "password123";

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword, workFactor: 12);

        var adminUser = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = adminEmail,
            PasswordHash = passwordHash,
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("DataSeeder: Seeded admin user ({Email})", adminEmail);
    }

    private static async Task SeedFeaturesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Features.AnyAsync())
        {
            logger.LogInformation("DataSeeder: Features already seeded — skipping");
            return;
        }

        var features = new List<Feature>
        {
            new() { Name = "Dark Mode", CreatedAt = DateTime.UtcNow },
            new() { Name = "Two-Factor Authentication", CreatedAt = DateTime.UtcNow },
            new() { Name = "Export to CSV", CreatedAt = DateTime.UtcNow },
            new() { Name = "Email Notifications", CreatedAt = DateTime.UtcNow },
            new() { Name = "API Rate Limiting", CreatedAt = DateTime.UtcNow },
        };

        context.Features.AddRange(features);
        await context.SaveChangesAsync();

        logger.LogInformation("DataSeeder: Seeded {Count} features", features.Count);
    }
}
