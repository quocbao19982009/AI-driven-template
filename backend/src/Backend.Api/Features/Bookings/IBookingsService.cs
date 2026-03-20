using Backend.Common.Models;

namespace Backend.Features.Bookings;

public interface IBookingsService
{
    Task<PagedResult<BookingDto>> GetAllAsync(GetBookingsQuery query, CancellationToken cancellationToken = default);
    Task<BookingDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingDto> UpdateAsync(int id, UpdateBookingRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
