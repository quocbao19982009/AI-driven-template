namespace Backend.Features.Flashcards;

public interface IFlashcardsRepository
{
    Task<(List<Flashcard> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        int? categoryId,
        CancellationToken cancellationToken = default);

    Task<Flashcard?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountByCategoryAsync(int categoryId, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<Flashcard> CreateAsync(Flashcard entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Flashcard entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Flashcard entity, CancellationToken cancellationToken = default);
}
