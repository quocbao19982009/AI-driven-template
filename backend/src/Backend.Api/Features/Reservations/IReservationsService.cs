using Backend.Common.Models;

namespace Backend.Features.Reservations;

public interface IReservationsService
{
    Task<PagedResult<ReservationDto>> GetAllAsync(
        int page, int pageSize,
        int? factoryId, int? personId,
        DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<ReservationDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);
    Task<ReservationDto> UpdateAsync(int id, UpdateReservationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
