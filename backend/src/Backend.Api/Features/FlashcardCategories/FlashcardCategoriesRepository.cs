using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.FlashcardCategories;

public class FlashcardCategoriesRepository : IFlashcardCategoriesRepository
{
    private readonly ApplicationDbContext _context;

    public FlashcardCategoriesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<FlashcardCategory> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FlashcardCategories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(lower));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<FlashcardCategory>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<FlashcardCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.FlashcardCategories.FindAsync([id], cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FlashcardCategories.AsNoTracking()
            .Where(c => c.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<FlashcardCategory> CreateAsync(FlashcardCategory entity, CancellationToken cancellationToken = default)
    {
        _context.FlashcardCategories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(FlashcardCategory entity, CancellationToken cancellationToken = default)
    {
        _context.FlashcardCategories.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(FlashcardCategory entity, CancellationToken cancellationToken = default)
    {
        _context.FlashcardCategories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
