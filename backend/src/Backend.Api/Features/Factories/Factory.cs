using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;

namespace Backend.Features.Factories;

public class Factory : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string TimeZone { get; set; } = string.Empty;
}
