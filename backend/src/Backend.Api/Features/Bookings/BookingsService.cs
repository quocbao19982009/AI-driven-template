using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features.Bookings;

public class BookingsService : IBookingsService
{
    private readonly IBookingsRepository _repository;
    private readonly IValidator<CreateBookingRequest> _createValidator;
    private readonly IValidator<UpdateBookingRequest> _updateValidator;
    private readonly ILogger<BookingsService> _logger;

    public BookingsService(
        IBookingsRepository repository,
        IValidator<CreateBookingRequest> createValidator,
        IValidator<UpdateBookingRequest> updateValidator,
        ILogger<BookingsService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<BookingDto>> GetAllAsync(BookingsListQuery query, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (query.Page < 1) errors.Add("Page must be at least 1.");
        if (query.PageSize < 1 || query.PageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);

        var (items, totalCount) = await _repository.GetAllAsync(query, cancellationToken);
        return new PagedResult<BookingDto>(items.Select(MapToDto).ToList(), totalCount, query.Page, query.PageSize);
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

        if (await _repository.HasOverlapAsync(request.RoomId, request.StartTime, request.EndTime, null, cancellationToken))
            throw new Common.Exceptions.ValidationException(["Room is already booked for the requested time slot."]);

        var entity = new Booking
        {
            RoomId = request.RoomId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BookedBy = request.BookedBy,
            Purpose = request.Purpose,
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Booking {BookingId}", created.Id);
        return MapToDto(created);
    }

    public async Task<BookingDto> UpdateAsync(int id, UpdateBookingRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Booking {BookingId} not found for update", id);
            throw new NotFoundException("Booking", id);
        }

        if (await _repository.HasOverlapAsync(request.RoomId, request.StartTime, request.EndTime, id, cancellationToken))
            throw new Common.Exceptions.ValidationException(["Room is already booked for the requested time slot."]);

        entity.RoomId = request.RoomId;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.BookedBy = request.BookedBy;
        entity.Purpose = request.Purpose;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Booking {BookingId}", entity.Id);
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

    private static BookingDto MapToDto(Booking b) => new(
        b.Id, b.RoomId, b.Room?.Name ?? string.Empty,
        b.StartTime, b.EndTime, b.BookedBy, b.Purpose, b.CreatedAt);

    private static async Task ValidateAndThrowAsync<T>(IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            throw new Common.Exceptions.ValidationException([.. result.Errors.Select(e => e.ErrorMessage)]);
    }
}
