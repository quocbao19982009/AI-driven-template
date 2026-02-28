namespace Backend.Features.Flashcards;

public record FlashcardDto(
    int Id,
    string FinnishWord,
    string EnglishTranslation,
    string Category,
    DateTime? NextReviewDate,
    DateTime? LastReviewedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateFlashcardRequest
{
    public string FinnishWord { get; set; } = string.Empty;
    public string EnglishTranslation { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class UpdateFlashcardRequest
{
    public string FinnishWord { get; set; } = string.Empty;
    public string EnglishTranslation { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
