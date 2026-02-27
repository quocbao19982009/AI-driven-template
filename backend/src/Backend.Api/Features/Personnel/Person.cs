using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;
using Backend.Features.Factories;

namespace Backend.Features.Personnel;

public class Person : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string PersonalId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Email { get; set; } = string.Empty;

    public ICollection<Factory> AllowedFactories { get; set; } = [];
}
