namespace Backend.Features.Reservations;

public interface IReservationsRepository
{
    Task<(List<Reservation> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        int? factoryId,
        int? personId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingReservationAsync(
        List<int> personIds,
        DateTime startTime,
        DateTime endTime,
        int? excludeReservationId = null,
        CancellationToken cancellationToken = default);

    Task<Reservation> CreateAsync(Reservation entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reservation entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Reservation entity, CancellationToken cancellationToken = default);
}
