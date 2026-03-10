using System.ComponentModel.DataAnnotations;

namespace Backend.Features._FeatureTemplate;

// TODO: Rename all "Feature" references to your entity name

public record FeatureDto(
    int Id,
    string Name,
    DateTime CreatedAt
);

public class CreateFeatureRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    // TODO: Add create properties
}

public class UpdateFeatureRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    // TODO: Add update properties
}
