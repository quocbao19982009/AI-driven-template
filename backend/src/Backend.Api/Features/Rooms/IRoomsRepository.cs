using Backend.Common.Models;

namespace Backend.Features.Rooms;

public interface IRoomsRepository
{
    Task<(List<Room> Items, int TotalCount)> GetAllAsync(RoomsListQuery query, CancellationToken cancellationToken = default);
    Task<List<Room>> GetAllUnpagedAsync(CancellationToken cancellationToken = default);
    Task<Room?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasBookingsAsync(int roomId, CancellationToken cancellationToken = default);
    Task<Room> CreateAsync(Room entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Room entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Room entity, CancellationToken cancellationToken = default);
}
