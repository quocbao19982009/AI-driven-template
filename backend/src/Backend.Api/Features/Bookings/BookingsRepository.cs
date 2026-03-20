using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Bookings;

public class BookingsRepository : IBookingsRepository
{
    private readonly ApplicationDbContext _context;

    public BookingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, int? roomId, DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Room)
            .AsQueryable();

        if (roomId.HasValue)
            query = query.Where(b => b.RoomId == roomId.Value);

        if (fromDate.HasValue)
            query = query.Where(b => b.EndTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.StartTime <= toDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(b => b.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Booking> CreateAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await _context.Entry(entity).Reference(b => b.Room).LoadAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(
        int roomId, DateTime startTime, DateTime endTime, int? excludeId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.RoomId == roomId
                && b.StartTime < endTime
                && b.EndTime > startTime);

        if (excludeId.HasValue)
            query = query.Where(b => b.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsForRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b => b.RoomId == roomId, cancellationToken);
    }
}
