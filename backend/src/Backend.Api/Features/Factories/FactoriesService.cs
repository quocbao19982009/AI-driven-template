using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features.Factories;

public class FactoriesService : IFactoriesService
{
    private readonly IFactoriesRepository _repository;
    private readonly IValidator<CreateFactoryRequest> _createValidator;
    private readonly IValidator<UpdateFactoryRequest> _updateValidator;
    private readonly ILogger<FactoriesService> _logger;

    public FactoriesService(
        IFactoriesRepository repository,
        IValidator<CreateFactoryRequest> createValidator,
        IValidator<UpdateFactoryRequest> updateValidator,
        ILogger<FactoriesService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<FactoryDto>> GetAllAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (page < 1) errors.Add("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(page, pageSize, search, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return new PagedResult<FactoryDto>(items, totalCount, page, pageSize);
    }

    public async Task<List<FactoryDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllUnpagedAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<FactoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Factory {FactoryId} not found", id);
            throw new NotFoundException("Factory", id);
        }
        return MapToDto(entity);
    }

    public async Task<FactoryDto> CreateAsync(CreateFactoryRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        if (await _repository.ExistsWithNameAsync(request.Name, cancellationToken: cancellationToken))
            throw new Common.Exceptions.ValidationException([$"A factory with the name '{request.Name}' already exists."]);

        var entity = new Factory
        {
            Name = request.Name,
            TimeZone = request.TimeZone
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Factory {FactoryId} with name {Name}", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<FactoryDto> UpdateAsync(int id, UpdateFactoryRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Factory {FactoryId} not found for update", id);
            throw new NotFoundException("Factory", id);
        }

        if (await _repository.ExistsWithNameAsync(request.Name, excludeId: id, cancellationToken: cancellationToken))
            throw new Common.Exceptions.ValidationException([$"A factory with the name '{request.Name}' already exists."]);

        entity.Name = request.Name;
        entity.TimeZone = request.TimeZone;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Factory {FactoryId}", entity.Id);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Factory {FactoryId} not found for deletion", id);
            throw new NotFoundException("Factory", id);
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Factory {FactoryId}", id);
    }

    internal static FactoryDto MapToDto(Factory entity) => new(
        entity.Id,
        entity.Name,
        entity.TimeZone,
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
