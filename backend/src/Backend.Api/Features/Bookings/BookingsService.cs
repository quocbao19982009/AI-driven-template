using Backend.Common.Exceptions;
using Backend.Common.Models;
using Backend.Features.Rooms;
using FluentValidation;

namespace Backend.Features.Bookings;

public class BookingsService : IBookingsService
{
    private readonly IBookingsRepository _repository;
    private readonly IRoomsRepository _roomsRepository;
    private readonly IValidator<CreateBookingRequest> _createValidator;
    private readonly IValidator<UpdateBookingRequest> _updateValidator;
    private readonly ILogger<BookingsService> _logger;

    public BookingsService(
        IBookingsRepository repository,
        IRoomsRepository roomsRepository,
        IValidator<CreateBookingRequest> createValidator,
        IValidator<UpdateBookingRequest> updateValidator,
        ILogger<BookingsService> logger)
    {
        _repository = repository;
        _roomsRepository = roomsRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<BookingDto>> GetAllAsync(GetBookingsQuery query, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (query.Page < 1) errors.Add("Page must be greater than or equal to 1.");
        if (query.PageSize < 1 || query.PageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Backend.Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(
            query.Page, query.PageSize, query.RoomId, query.FromDate, query.ToDate, cancellationToken);

        var items = entities.Select(MapToDto).ToList();
        _logger.LogDebug("Retrieved {Count} of {Total} Bookings (page {Page})", items.Count, totalCount, query.Page);
        return new PagedResult<BookingDto>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<BookingDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Booking {BookingId} not found", id);
            throw new NotFoundException("Booking", id);
        }

        return MapToDto(entity);
    }

    public async Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        // Business rule: StartTime must be before EndTime
        if (request.StartTime >= request.EndTime)
            throw new Backend.Common.Exceptions.ValidationException(["StartTime must be before EndTime."]);

        // Validate room exists
        var room = await _roomsRepository.GetByIdAsync(request.RoomId, cancellationToken);
        if (room is null)
            throw new Backend.Common.Exceptions.ValidationException([$"Room with id {request.RoomId} does not exist."]);

        // Business rule: No overlapping bookings
        var hasOverlap = await _repository.HasOverlapAsync(
            request.RoomId, request.StartTime, request.EndTime, excludeId: null, cancellationToken);
        if (hasOverlap)
            throw new Backend.Common.Exceptions.ValidationException(["This room is already booked for the requested time range."]);

        var entity = new Booking
        {
            RoomId = request.RoomId,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime(),
            BookedBy = request.BookedBy,
            Purpose = request.Purpose
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Booking {BookingId} for Room {RoomId}", created.Id, created.RoomId);
        return MapToDto(created);
    }

    public async Task<BookingDto> UpdateAsync(int id, UpdateBookingRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        // Business rule: StartTime must be before EndTime
        if (request.StartTime >= request.EndTime)
            throw new Backend.Common.Exceptions.ValidationException(["StartTime must be before EndTime."]);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Booking {BookingId} not found for update", id);
            throw new NotFoundException("Booking", id);
        }

        // Validate room exists
        var room = await _roomsRepository.GetByIdAsync(request.RoomId, cancellationToken);
        if (room is null)
            throw new Backend.Common.Exceptions.ValidationException([$"Room with id {request.RoomId} does not exist."]);

        // Business rule: No overlapping bookings (excluding self)
        var hasOverlap = await _repository.HasOverlapAsync(
            request.RoomId, request.StartTime, request.EndTime, excludeId: id, cancellationToken);
        if (hasOverlap)
            throw new Backend.Common.Exceptions.ValidationException(["This room is already booked for the requested time range."]);

        entity.RoomId = request.RoomId;
        entity.StartTime = request.StartTime.ToUniversalTime();
        entity.EndTime = request.EndTime.ToUniversalTime();
        entity.BookedBy = request.BookedBy;
        entity.Purpose = request.Purpose;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Booking {BookingId}", entity.Id);

        entity.Room = room;
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Booking {BookingId} not found for deletion", id);
            throw new NotFoundException("Booking", id);
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Booking {BookingId}", id);
    }

    private static BookingDto MapToDto(Booking entity) => new(
        entity.Id,
        entity.RoomId,
        entity.Room?.Name ?? string.Empty,
        entity.StartTime,
        entity.EndTime,
        entity.BookedBy,
        entity.Purpose,
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
