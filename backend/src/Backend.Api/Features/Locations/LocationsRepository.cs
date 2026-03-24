using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Locations;

public class LocationsRepository : ILocationsRepository
{
    private readonly ApplicationDbContext _context;

    public LocationsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Locations.FindAsync([id], cancellationToken);
    }

    public async Task<Location> CreateAsync(Location entity, CancellationToken cancellationToken = default)
    {
        _context.Locations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Location entity, CancellationToken cancellationToken = default)
    {
        _context.Locations.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Location entity, CancellationToken cancellationToken = default)
    {
        _context.Locations.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasRoomsAsync(int locationId, CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .AnyAsync(r => r.LocationId == locationId, cancellationToken);
    }
}
