using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Reservations;

public class ReservationsRepository : IReservationsRepository
{
    private readonly ApplicationDbContext _context;

    public ReservationsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Reservation> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        int? factoryId,
        int? personId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Reservations
            .AsNoTracking()
            .Include(r => r.ReservationPersonnel)
            .AsQueryable();

        if (factoryId.HasValue)
            query = query.Where(r => r.FactoryId == factoryId.Value);

        if (personId.HasValue)
            query = query.Where(r => r.ReservationPersonnel.Any(rp => rp.PersonId == personId.Value));

        if (fromDate.HasValue)
            query = query.Where(r => r.EndTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.StartTime <= toDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.ReservationPersonnel)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> HasOverlappingReservationAsync(
        List<int> personIds,
        DateTime startTime,
        DateTime endTime,
        int? excludeReservationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ReservationPersonnel
            .AsNoTracking()
            .Where(rp =>
                rp.PersonId != null &&
                personIds.Contains(rp.PersonId.Value) &&
                rp.Reservation.EndTime > startTime &&
                rp.Reservation.StartTime < endTime);

        if (excludeReservationId.HasValue)
            query = query.Where(rp => rp.ReservationId != excludeReservationId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Reservation> CreateAsync(Reservation entity, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Reservation entity, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Reservation entity, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
