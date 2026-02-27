using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Factories;

public class FactoriesRepository : IFactoriesRepository
{
    private readonly ApplicationDbContext _context;

    public FactoriesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Factory> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Factories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(f => f.Name.ToLower().Contains(lower));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Factory>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Factories
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Factory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Factories.FindAsync([id], cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Factories.AsNoTracking()
            .Where(f => f.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
            query = query.Where(f => f.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Factory> CreateAsync(Factory entity, CancellationToken cancellationToken = default)
    {
        _context.Factories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Factory entity, CancellationToken cancellationToken = default)
    {
        _context.Factories.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Factory entity, CancellationToken cancellationToken = default)
    {
        _context.Factories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
