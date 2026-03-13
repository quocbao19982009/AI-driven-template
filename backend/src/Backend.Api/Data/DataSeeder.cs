using Backend.Features._FeatureTemplate;
using Backend.Features.ExpenseTrackers;
using Backend.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, IConfiguration configuration)
    {
        logger.LogInformation("DataSeeder: Running development seed...");

        await SeedAdminUserAsync(context, logger, configuration);
        await SeedTestUsersAsync(context, logger);
        await SeedFeaturesAsync(context, logger);
        await SeedExpenseTrackersAsync(context, logger);
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

    private static async Task SeedTestUsersAsync(ApplicationDbContext context, ILogger logger)
    {
        var testEmails = new[] { "user1@example.com", "user2@example.com", "user3@example.com" };

        if (await context.Users.AnyAsync(u => testEmails.Contains(u.Email)))
        {
            logger.LogInformation("DataSeeder: Test users already exist — skipping");
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123", workFactor: 12);

        var testUsers = new List<User>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "user1@example.com", PasswordHash = passwordHash, Role = "User", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { FirstName = "Bob", LastName = "Smith", Email = "user2@example.com", PasswordHash = passwordHash, Role = "User", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { FirstName = "Carol", LastName = "Williams", Email = "user3@example.com", PasswordHash = passwordHash, Role = "User", IsActive = true, CreatedAt = DateTime.UtcNow },
        };

        context.Users.AddRange(testUsers);
        await context.SaveChangesAsync();

        logger.LogInformation("DataSeeder: Seeded {Count} test users", testUsers.Count);
    }

    private static async Task SeedExpenseTrackersAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.ExpenseTrackers.AnyAsync())
        {
            logger.LogInformation("DataSeeder: ExpenseTrackers already seeded — skipping");
            return;
        }

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
        var user1 = await context.Users.FirstOrDefaultAsync(u => u.Email == "user1@example.com");
        var user2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "user2@example.com");
        var user3 = await context.Users.FirstOrDefaultAsync(u => u.Email == "user3@example.com");

        if (admin is null || user1 is null || user2 is null || user3 is null)
        {
            logger.LogWarning("DataSeeder: Cannot seed expenses — required users not found");
            return;
        }

        var expenses = new List<ExpenseTracker>
        {
            new() { Amount = 12.50m, Category = "Food", Description = "Lunch at cafe", Date = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), UserId = user1.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 45.00m, Category = "Transport", Description = "Taxi to airport", Date = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc), UserId = user1.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 120.00m, Category = "Utilities", Description = "Electricity bill", Date = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc), UserId = user2.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 8.99m, Category = "Entertainment", Description = "Movie ticket", Date = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc), UserId = user2.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 25.00m, Category = "Food", Description = "Grocery shopping", Date = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc), UserId = user3.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 15.50m, Category = "Other", Description = "Office supplies", Date = new DateTime(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc), UserId = user3.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 200.00m, Category = "Transport", Description = "Monthly bus pass", Date = new DateTime(2026, 3, 7, 0, 0, 0, DateTimeKind.Utc), UserId = admin.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 55.00m, Category = "Entertainment", Description = "Concert tickets", Date = new DateTime(2026, 3, 8, 0, 0, 0, DateTimeKind.Utc), UserId = admin.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 30.00m, Category = "Food", Description = "Team dinner", Date = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc), UserId = user1.Id, CreatedAt = DateTime.UtcNow },
            new() { Amount = 75.00m, Category = "Utilities", Description = "Internet bill", Date = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc), UserId = user2.Id, CreatedAt = DateTime.UtcNow },
        };

        context.ExpenseTrackers.AddRange(expenses);
        await context.SaveChangesAsync();

        logger.LogInformation("DataSeeder: Seeded {Count} expense trackers", expenses.Count);
    }
}
