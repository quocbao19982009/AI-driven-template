using Backend.Common.Models;

namespace Backend.Features.Flashcards;

public interface IFlashcardsService
{
    Task<PagedResult<FlashcardDto>> GetAllAsync(int page, int pageSize, string? search, int? categoryId, CancellationToken cancellationToken = default);
    Task<FlashcardDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FlashcardDto> CreateAsync(CreateFlashcardRequest request, CancellationToken cancellationToken = default);
    Task<FlashcardDto> UpdateAsync(int id, UpdateFlashcardRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<FlashcardDto> MarkReviewedAsync(int id, CancellationToken cancellationToken = default);
}
