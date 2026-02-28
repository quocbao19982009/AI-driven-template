using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features.FlashcardCategories;

public class FlashcardCategoriesService : IFlashcardCategoriesService
{
    private readonly IFlashcardCategoriesRepository _repository;
    private readonly IValidator<CreateFlashcardCategoryRequest> _createValidator;
    private readonly IValidator<UpdateFlashcardCategoryRequest> _updateValidator;
    private readonly ILogger<FlashcardCategoriesService> _logger;

    public FlashcardCategoriesService(
        IFlashcardCategoriesRepository repository,
        IValidator<CreateFlashcardCategoryRequest> createValidator,
        IValidator<UpdateFlashcardCategoryRequest> updateValidator,
        ILogger<FlashcardCategoriesService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<FlashcardCategoryDto>> GetAllAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (page < 1) errors.Add("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(page, pageSize, search, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return new PagedResult<FlashcardCategoryDto>(items, totalCount, page, pageSize);
    }

    public async Task<List<FlashcardCategoryDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllUnpagedAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<FlashcardCategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("FlashcardCategory {CategoryId} not found", id);
            throw new NotFoundException("FlashcardCategory", id);
        }
        return MapToDto(entity);
    }

    public async Task<FlashcardCategoryDto> CreateAsync(CreateFlashcardCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        if (await _repository.ExistsWithNameAsync(request.Name, cancellationToken: cancellationToken))
            throw new Common.Exceptions.ValidationException([$"A flashcard category with the name '{request.Name}' already exists."]);

        var entity = new FlashcardCategory
        {
            Name = request.Name
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created FlashcardCategory {CategoryId} with name {Name}", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<FlashcardCategoryDto> UpdateAsync(int id, UpdateFlashcardCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("FlashcardCategory {CategoryId} not found for update", id);
            throw new NotFoundException("FlashcardCategory", id);
        }

        if (await _repository.ExistsWithNameAsync(request.Name, excludeId: id, cancellationToken: cancellationToken))
            throw new Common.Exceptions.ValidationException([$"A flashcard category with the name '{request.Name}' already exists."]);

        entity.Name = request.Name;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated FlashcardCategory {CategoryId}", entity.Id);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("FlashcardCategory {CategoryId} not found for deletion", id);
            throw new NotFoundException("FlashcardCategory", id);
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted FlashcardCategory {CategoryId}", id);
    }

    internal static FlashcardCategoryDto MapToDto(FlashcardCategory entity) => new(
        entity.Id,
        entity.Name,
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
