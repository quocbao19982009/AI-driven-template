using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;

namespace Backend.Features.Flashcards;

public class Flashcard : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string FinnishWord { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string EnglishTranslation { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public DateTime? NextReviewDate { get; set; }

    public DateTime? LastReviewedAt { get; set; }
}
