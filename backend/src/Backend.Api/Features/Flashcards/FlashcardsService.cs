using Backend.Common.Exceptions;
using Backend.Common.Models;
using Backend.Features.FlashcardCategories;
using FluentValidation;

namespace Backend.Features.Flashcards;

public class FlashcardsService : IFlashcardsService
{
    private const int MaxCardsPerCategory = 10;

    private readonly IFlashcardsRepository _repository;
    private readonly IFlashcardCategoriesRepository _categoriesRepository;
    private readonly IValidator<CreateFlashcardRequest> _createValidator;
    private readonly IValidator<UpdateFlashcardRequest> _updateValidator;
    private readonly ILogger<FlashcardsService> _logger;

    public FlashcardsService(
        IFlashcardsRepository repository,
        IFlashcardCategoriesRepository categoriesRepository,
        IValidator<CreateFlashcardRequest> createValidator,
        IValidator<UpdateFlashcardRequest> updateValidator,
        ILogger<FlashcardsService> logger)
    {
        _repository = repository;
        _categoriesRepository = categoriesRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<FlashcardDto>> GetAllAsync(
        int page, int pageSize, string? search, int? categoryId, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (page < 1) errors.Add("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(page, pageSize, search, categoryId, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return new PagedResult<FlashcardDto>(items, totalCount, page, pageSize);
    }

    public async Task<FlashcardDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Flashcard {FlashcardId} not found", id);
            throw new NotFoundException("Flashcard", id);
        }
        return MapToDto(entity);
    }

    public async Task<FlashcardDto> CreateAsync(CreateFlashcardRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        if (request.CategoryId.HasValue)
        {
            var category = await _categoriesRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category is null)
                throw new Common.Exceptions.ValidationException([$"Category with ID {request.CategoryId.Value} does not exist."]);

            var count = await _repository.CountByCategoryAsync(request.CategoryId.Value, cancellationToken: cancellationToken);
            if (count >= MaxCardsPerCategory)
                throw new Common.Exceptions.ValidationException([$"Category '{category.Name}' already has {MaxCardsPerCategory} cards. Remove a card before adding a new one."]);
        }

        var entity = new Flashcard
        {
            FinnishWord = request.FinnishWord,
            EnglishTranslation = request.EnglishTranslation,
            CategoryId = request.CategoryId
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Flashcard {FlashcardId} in category {CategoryId}", created.Id, created.CategoryId);
        return MapToDto(created);
    }

    public async Task<FlashcardDto> UpdateAsync(int id, UpdateFlashcardRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Flashcard {FlashcardId} not found for update", id);
            throw new NotFoundException("Flashcard", id);
        }

        if (request.CategoryId.HasValue && request.CategoryId != entity.CategoryId)
        {
            var category = await _categoriesRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category is null)
                throw new Common.Exceptions.ValidationException([$"Category with ID {request.CategoryId.Value} does not exist."]);

            var count = await _repository.CountByCategoryAsync(request.CategoryId.Value, excludeId: id, cancellationToken: cancellationToken);
            if (count >= MaxCardsPerCategory)
                throw new Common.Exceptions.ValidationException([$"Category '{category.Name}' already has {MaxCardsPerCategory} cards. Remove a card before moving this one."]);
        }

        entity.FinnishWord = request.FinnishWord;
        entity.EnglishTranslation = request.EnglishTranslation;
        entity.CategoryId = request.CategoryId;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Flashcard {FlashcardId}", entity.Id);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Flashcard {FlashcardId} not found for deletion", id);
            throw new NotFoundException("Flashcard", id);
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Flashcard {FlashcardId}", id);
    }

    public async Task<FlashcardDto> MarkReviewedAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Flashcard {FlashcardId} not found for review", id);
            throw new NotFoundException("Flashcard", id);
        }

        entity.LastReviewedAt = DateTime.UtcNow;
        entity.NextReviewDate = null;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Marked Flashcard {FlashcardId} as reviewed", id);
        return MapToDto(entity);
    }

    internal static FlashcardDto MapToDto(Flashcard entity) => new(
        entity.Id,
        entity.FinnishWord,
        entity.EnglishTranslation,
        entity.CategoryId,
        entity.Category?.Name,
        entity.NextReviewDate,
        entity.LastReviewedAt,
        entity.CreatedAt,
        entity.UpdatedAt
    );

    private static async Task ValidateAndThrowAsync<T>(
        IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new Common.Exceptions.ValidationException(
                [.. validation.Errors.Select(e => e.ErrorMessage)]);
        }
    }
}
