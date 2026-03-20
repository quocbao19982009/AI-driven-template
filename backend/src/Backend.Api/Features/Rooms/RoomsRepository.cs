using Backend.Common.Models;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Rooms;

public class RoomsRepository : IRoomsRepository
{
    private readonly ApplicationDbContext _context;

    public RoomsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Room> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, string? search, int? locationId,
        string sortBy, string sortDir, CancellationToken cancellationToken = default)
    {
        var query = _context.Rooms
            .AsNoTracking()
            .Include(r => r.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name.ToLower().Contains(search.ToLower()));

        if (locationId.HasValue)
            query = query.Where(r => r.LocationId == locationId.Value);

        query = (sortBy.ToLower(), sortDir.ToLower()) switch
        {
            ("capacity", "desc") => query.OrderByDescending(r => r.Capacity),
            ("capacity", _) => query.OrderBy(r => r.Capacity),
            ("createdat", "desc") => query.OrderByDescending(r => r.CreatedAt),
            ("createdat", _) => query.OrderBy(r => r.CreatedAt),
            (_, "desc") => query.OrderByDescending(r => r.Name),
            _ => query.OrderBy(r => r.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Room>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Include(r => r.Location)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Room?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .Include(r => r.Location)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Room> CreateAsync(Room entity, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        // Reload with navigation properties
        await _context.Entry(entity).Reference(r => r.Location).LoadAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Room entity, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Room entity, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasBookingsAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b => b.RoomId == roomId, cancellationToken);
    }
}
