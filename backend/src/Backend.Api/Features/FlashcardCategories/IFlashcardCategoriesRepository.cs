namespace Backend.Features.FlashcardCategories;

public interface IFlashcardCategoriesRepository
{
    Task<(List<FlashcardCategory> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<List<FlashcardCategory>> GetAllUnpagedAsync(CancellationToken cancellationToken = default);
    Task<FlashcardCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<FlashcardCategory> CreateAsync(FlashcardCategory entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FlashcardCategory entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(FlashcardCategory entity, CancellationToken cancellationToken = default);
}
