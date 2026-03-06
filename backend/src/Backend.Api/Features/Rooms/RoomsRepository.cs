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
        RoomsListQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Rooms.AsNoTracking().Include(r => r.Location).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(r => r.Name.ToLower().Contains(query.Search.ToLower()));

        if (query.LocationId.HasValue)
            q = q.Where(r => r.LocationId == query.LocationId.Value);

        q = (query.SortBy.ToLower(), query.SortDir.ToLower()) switch
        {
            ("name", "desc") => q.OrderByDescending(r => r.Name),
            ("name", _) => q.OrderBy(r => r.Name),
            ("capacity", "desc") => q.OrderByDescending(r => r.Capacity),
            ("capacity", _) => q.OrderBy(r => r.Capacity),
            ("createdat", "desc") => q.OrderByDescending(r => r.CreatedAt),
            _ => q.OrderBy(r => r.CreatedAt),
        };

        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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

    public async Task<bool> HasBookingsAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings.AnyAsync(b => b.RoomId == roomId, cancellationToken);
    }

    public async Task<Room> CreateAsync(Room entity, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
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
}
