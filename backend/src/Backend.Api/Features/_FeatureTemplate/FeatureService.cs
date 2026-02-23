using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features._FeatureTemplate;

// TODO: Rename to match your entity (e.g., ProductsService)

public class FeatureService : IFeatureService
{
    private readonly IFeatureRepository _repository;
    private readonly IValidator<CreateFeatureRequest> _createValidator;
    private readonly IValidator<UpdateFeatureRequest> _updateValidator;
    private readonly ILogger<FeatureService> _logger;

    public FeatureService(
        IFeatureRepository repository,
        IValidator<CreateFeatureRequest> createValidator,
        IValidator<UpdateFeatureRequest> updateValidator,
        ILogger<FeatureService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<FeatureDto>> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (page < 1)
            errors.Add("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100)
            errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0)
            throw new Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        _logger.LogDebug("Retrieved {Count} of {Total} Features (page {Page})", items.Count, totalCount, page);
        return new PagedResult<FeatureDto>(items, totalCount, page, pageSize);
    }

    public async Task<FeatureDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Feature {FeatureId} not found", id);
            throw new NotFoundException("Feature", id);
        }

        return MapToDto(entity);
    }

    public async Task<FeatureDto> CreateAsync(CreateFeatureRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        var entity = new Feature
        {
            Name = request.Name
            // TODO: Map additional properties
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Feature {FeatureId} with name {FeatureName}", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<FeatureDto> UpdateAsync(int id, UpdateFeatureRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Feature {FeatureId} not found for update", id);
            throw new NotFoundException("Feature", id);
        }

        entity.Name = request.Name;
        // TODO: Map additional properties

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Feature {FeatureId}", entity.Id);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Feature {FeatureId} not found for deletion", id);
            throw new NotFoundException("Feature", id);
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Feature {FeatureId}", id);
    }

    private static FeatureDto MapToDto(Feature entity) => new(
        entity.Id,
        entity.Name,
        entity.CreatedAt
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
