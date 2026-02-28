using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;
using Backend.Features.FlashcardCategories;

namespace Backend.Features.Flashcards;

public class Flashcard : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string FinnishWord { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string EnglishTranslation { get; set; } = string.Empty;

    public int? CategoryId { get; set; }
    public FlashcardCategory? Category { get; set; }

    public DateTime? NextReviewDate { get; set; }

    public DateTime? LastReviewedAt { get; set; }
}
