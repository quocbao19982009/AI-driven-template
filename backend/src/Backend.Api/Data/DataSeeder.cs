using Backend.Features._FeatureTemplate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("DataSeeder: Running development seed...");

        await SeedFeaturesAsync(context, logger);
        // TODO: Add new seed methods here as you add new features
        // await SeedProductsAsync(context, logger);
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
