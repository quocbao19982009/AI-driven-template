using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.ExpenseTrackers;

public class ExpenseTrackersRepository : IExpenseTrackersRepository
{
    private readonly ApplicationDbContext _context;

    public ExpenseTrackersRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<ExpenseTracker> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ExpenseTrackers
            .Include(e => e.User)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ExpenseTracker?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseTrackers
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<ExpenseTracker> CreateAsync(ExpenseTracker entity, CancellationToken cancellationToken = default)
    {
        _context.ExpenseTrackers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with User navigation for DTO mapping
        await _context.Entry(entity).Reference(e => e.User).LoadAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(ExpenseTracker entity, CancellationToken cancellationToken = default)
    {
        _context.ExpenseTrackers.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ExpenseTracker entity, CancellationToken cancellationToken = default)
    {
        _context.ExpenseTrackers.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
