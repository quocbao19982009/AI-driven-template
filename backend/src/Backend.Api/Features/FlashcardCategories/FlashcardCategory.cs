using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;

namespace Backend.Features.FlashcardCategories;

public class FlashcardCategory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
