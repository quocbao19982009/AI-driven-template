namespace Backend.Features.Bookings;

public interface IBookingsRepository
{
    Task<(List<Booking> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, int? roomId, DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Booking> CreateAsync(Booking entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Booking entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Booking entity, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForRoomAsync(int roomId, CancellationToken cancellationToken = default);
}
