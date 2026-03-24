namespace Backend.Features.Locations;

public interface ILocationsService
{
    Task<List<LocationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LocationDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LocationDto> CreateAsync(CreateLocationRequest request, CancellationToken cancellationToken = default);
    Task<LocationDto> UpdateAsync(int id, UpdateLocationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
