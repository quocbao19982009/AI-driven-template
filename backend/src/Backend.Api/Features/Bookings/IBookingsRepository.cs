namespace Backend.Features.Bookings;

public interface IBookingsRepository
{
    Task<(List<Booking> Items, int TotalCount)> GetAllAsync(BookingsListQuery query, CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId, CancellationToken cancellationToken = default);
    Task<Booking> CreateAsync(Booking entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Booking entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Booking entity, CancellationToken cancellationToken = default);
}
