using Backend.Common.Models;

namespace Backend.Features.FlashcardCategories;

public interface IFlashcardCategoriesService
{
    Task<PagedResult<FlashcardCategoryDto>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<List<FlashcardCategoryDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default);
    Task<FlashcardCategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FlashcardCategoryDto> CreateAsync(CreateFlashcardCategoryRequest request, CancellationToken cancellationToken = default);
    Task<FlashcardCategoryDto> UpdateAsync(int id, UpdateFlashcardCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
