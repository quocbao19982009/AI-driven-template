using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features.Locations;

public class LocationsService : ILocationsService
{
    private readonly ILocationsRepository _repository;
    private readonly IValidator<CreateLocationRequest> _createValidator;
    private readonly IValidator<UpdateLocationRequest> _updateValidator;
    private readonly ILogger<LocationsService> _logger;

    public LocationsService(
        ILocationsRepository repository,
        IValidator<CreateLocationRequest> createValidator,
        IValidator<UpdateLocationRequest> updateValidator,
        ILogger<LocationsService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<List<LocationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<LocationDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Location {LocationId} not found", id);
            throw new NotFoundException("Location", id);
        }

        return MapToDto(entity);
    }

    public async Task<LocationDto> CreateAsync(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        var entity = new Location
        {
            Name = request.Name
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Location {LocationId} with name {LocationName}", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<LocationDto> UpdateAsync(int id, UpdateLocationRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Location {LocationId} not found for update", id);
            throw new NotFoundException("Location", id);
        }

        entity.Name = request.Name;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Location {LocationId}", entity.Id);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Location {LocationId} not found for deletion", id);
            throw new NotFoundException("Location", id);
        }

        var hasRooms = await _repository.HasRoomsAsync(id, cancellationToken);
        if (hasRooms)
        {
            throw new Backend.Common.Exceptions.ValidationException(
                ["Cannot delete this location because it has rooms assigned to it."]);
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Location {LocationId}", id);
    }

    private static LocationDto MapToDto(Location entity) => new(
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
            throw new Backend.Common.Exceptions.ValidationException(
                [.. validation.Errors.Select(e => e.ErrorMessage)]);
        }
    }
}
