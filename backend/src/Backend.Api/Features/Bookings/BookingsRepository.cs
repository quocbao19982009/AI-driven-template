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
        BookingsListQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Bookings.AsNoTracking().Include(b => b.Room).AsQueryable();

        if (query.RoomId.HasValue)
            q = q.Where(b => b.RoomId == query.RoomId.Value);

        if (query.FromDate.HasValue)
            q = q.Where(b => b.StartTime >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(b => b.EndTime <= query.ToDate.Value);

        q = q.OrderBy(b => b.StartTime);

        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(
        int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId,
        CancellationToken cancellationToken = default)
    {
        var q = _context.Bookings.AsNoTracking()
            .Where(b => b.RoomId == roomId
                && b.StartTime < endTime
                && b.EndTime > startTime);

        if (excludeBookingId.HasValue)
            q = q.Where(b => b.Id != excludeBookingId.Value);

        return await q.AnyAsync(cancellationToken);
    }

    public async Task<Booking> CreateAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
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
}
