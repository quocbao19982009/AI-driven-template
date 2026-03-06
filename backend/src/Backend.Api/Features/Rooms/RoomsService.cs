using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features.Rooms;

public class RoomsService : IRoomsService
{
    private readonly IRoomsRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly IValidator<CreateRoomRequest> _createValidator;
    private readonly IValidator<UpdateRoomRequest> _updateValidator;
    private readonly ILogger<RoomsService> _logger;

    public RoomsService(
        IRoomsRepository repository,
        IFileStorageService fileStorage,
        IValidator<CreateRoomRequest> createValidator,
        IValidator<UpdateRoomRequest> updateValidator,
        ILogger<RoomsService> logger)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<RoomDto>> GetAllAsync(RoomsListQuery query, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (query.Page < 1) errors.Add("Page must be at least 1.");
        if (query.PageSize < 1 || query.PageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);

        var (items, totalCount) = await _repository.GetAllAsync(query, cancellationToken);
        return new PagedResult<RoomDto>(items.Select(MapToDto).ToList(), totalCount, query.Page, query.PageSize);
    }

    public async Task<List<RoomDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllUnpagedAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
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

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, IFormFile? image, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        string? imagePath = null;
        if (image is not null)
            imagePath = await _fileStorage.SaveRoomImageAsync(image, cancellationToken);

        var entity = new Room
        {
            Name = request.Name,
            Capacity = request.Capacity,
            LocationId = request.LocationId,
            Purpose = request.Purpose,
            ImagePath = imagePath,
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Room {RoomId}", created.Id);
        return MapToDto(created);
    }

    public async Task<RoomDto> UpdateAsync(int id, UpdateRoomRequest request, IFormFile? image, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Room {RoomId} not found for update", id);
            throw new NotFoundException("Room", id);
        }

        if (image is not null)
        {
            _fileStorage.DeleteRoomImage(entity.ImagePath);
            entity.ImagePath = await _fileStorage.SaveRoomImageAsync(image, cancellationToken);
        }

        entity.Name = request.Name;
        entity.Capacity = request.Capacity;
        entity.LocationId = request.LocationId;
        entity.Purpose = request.Purpose;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Room {RoomId}", entity.Id);
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

        if (await _repository.HasBookingsAsync(id, cancellationToken))
            throw new Common.Exceptions.ValidationException(["Room cannot be deleted because it has existing bookings."]);

        _fileStorage.DeleteRoomImage(entity.ImagePath);
        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Room {RoomId}", id);
    }

    private static RoomDto MapToDto(Room r) => new(
        r.Id, r.Name, r.Capacity, r.LocationId,
        r.Location?.Name ?? string.Empty,
        r.Purpose, r.ImagePath, r.CreatedAt);

    private static async Task ValidateAndThrowAsync<T>(IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            throw new Common.Exceptions.ValidationException([.. result.Errors.Select(e => e.ErrorMessage)]);
    }
}
