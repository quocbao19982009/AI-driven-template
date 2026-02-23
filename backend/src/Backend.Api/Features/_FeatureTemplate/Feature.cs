using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;

namespace Backend.Features._FeatureTemplate;

// TODO: Rename "Feature" to your entity name (e.g., Product)
public class Feature : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    // TODO: Add your entity properties here
}
