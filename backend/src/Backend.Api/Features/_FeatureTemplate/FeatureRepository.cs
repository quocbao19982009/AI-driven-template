using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features._FeatureTemplate;

// TODO: Rename to match your entity (e.g., ProductsRepository)

public class FeatureRepository : IFeatureRepository
{
    private readonly ApplicationDbContext _context;

    public FeatureRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // TODO: Replace _context.Set<Feature>() with _context.YourEntitySet
    // after adding DbSet to ApplicationDbContext

    public async Task<(List<Feature> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Feature>().AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Feature?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Feature>().FindAsync([id], cancellationToken);
    }

    public async Task<Feature> CreateAsync(Feature entity, CancellationToken cancellationToken = default)
    {
        _context.Set<Feature>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Feature entity, CancellationToken cancellationToken = default)
    {
        _context.Set<Feature>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Feature entity, CancellationToken cancellationToken = default)
    {
        _context.Set<Feature>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
