namespace Backend.Features.Locations;

public interface ILocationsRepository
{
    Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Location> CreateAsync(Location entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Location entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Location entity, CancellationToken cancellationToken = default);
    Task<bool> HasRoomsAsync(int locationId, CancellationToken cancellationToken = default);
}
