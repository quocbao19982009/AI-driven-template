namespace Backend.Features.FlashcardCategories;

public record FlashcardCategoryDto(
    int Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateFlashcardCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateFlashcardCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}
