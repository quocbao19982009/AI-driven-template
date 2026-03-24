using Backend.Common.Exceptions;
using Backend.Common.Models;
using Backend.Features.Locations;
using FluentValidation;

namespace Backend.Features.Rooms;

public class RoomsService : IRoomsService
{
    private readonly IRoomsRepository _repository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateRoomRequest> _createValidator;
    private readonly IValidator<UpdateRoomRequest> _updateValidator;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<RoomsService> _logger;

    public RoomsService(
        IRoomsRepository repository,
        ILocationsRepository locationsRepository,
        IValidator<CreateRoomRequest> createValidator,
        IValidator<UpdateRoomRequest> updateValidator,
        IWebHostEnvironment environment,
        ILogger<RoomsService> logger)
    {
        _repository = repository;
        _locationsRepository = locationsRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _environment = environment;
        _logger = logger;
    }

    public async Task<PagedResult<RoomDto>> GetAllAsync(GetRoomsQuery query, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (query.Page < 1) errors.Add("Page must be greater than or equal to 1.");
        if (query.PageSize < 1 || query.PageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Backend.Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(
            query.Page, query.PageSize, query.Search, query.LocationId,
            query.SortBy, query.SortDir, cancellationToken);

        var items = entities.Select(MapToDto).ToList();
        _logger.LogDebug("Retrieved {Count} of {Total} Rooms (page {Page})", items.Count, totalCount, query.Page);
        return new PagedResult<RoomDto>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<List<RoomDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllUnpagedAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<RoomDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Room {RoomId} not found", id);
            throw new NotFoundException("Room", id);
        }

        return MapToDto(entity);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        var location = await _locationsRepository.GetByIdAsync(request.LocationId, cancellationToken);
        if (location is null)
            throw new Backend.Common.Exceptions.ValidationException([$"Location with id {request.LocationId} does not exist."]);

        string? imagePath = null;
        if (request.Image is not null)
            imagePath = await SaveImageAsync(request.Image);

        var entity = new Room
        {
            Name = request.Name,
            Capacity = request.Capacity,
            LocationId = request.LocationId,
            Purpose = request.Purpose,
            ImagePath = imagePath
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Room {RoomId} with name {RoomName}", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<RoomDto> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Room {RoomId} not found for update", id);
            throw new NotFoundException("Room", id);
        }

        var location = await _locationsRepository.GetByIdAsync(request.LocationId, cancellationToken);
        if (location is null)
            throw new Backend.Common.Exceptions.ValidationException([$"Location with id {request.LocationId} does not exist."]);

        if (request.Image is not null)
        {
            // Delete old image if it exists
            if (!string.IsNullOrEmpty(entity.ImagePath))
                DeleteImageFile(entity.ImagePath);

            entity.ImagePath = await SaveImageAsync(request.Image);
        }

        entity.Name = request.Name;
        entity.Capacity = request.Capacity;
        entity.LocationId = request.LocationId;
        entity.Purpose = request.Purpose;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Room {RoomId}", entity.Id);

        // Reload with location for DTO mapping
        entity.Location = location;
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Room {RoomId} not found for deletion", id);
            throw new NotFoundException("Room", id);
        }

        var hasBookings = await _repository.HasBookingsAsync(id, cancellationToken);
        if (hasBookings)
        {
            throw new Backend.Common.Exceptions.ValidationException(
                ["Cannot delete this room because it has existing bookings."]);
        }

        if (!string.IsNullOrEmpty(entity.ImagePath))
            DeleteImageFile(entity.ImagePath);

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Room {RoomId}", id);
    }

    private async Task<string> SaveImageAsync(IFormFile image)
    {
        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "rooms");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await image.CopyToAsync(stream);

        return $"uploads/rooms/{fileName}";
    }

    private void DeleteImageFile(string imagePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, imagePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete image file at {ImagePath}", imagePath);
        }
    }

    private static RoomDto MapToDto(Room entity) => new(
        entity.Id,
        entity.Name,
        entity.Capacity,
        entity.LocationId,
        entity.Location?.Name ?? string.Empty,
        entity.Purpose,
        entity.ImagePath,
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
