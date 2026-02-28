using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Flashcards;

public class FlashcardsRepository : IFlashcardsRepository
{
    private readonly ApplicationDbContext _context;

    public FlashcardsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Flashcard> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Flashcards
            .Include(f => f.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(f =>
                f.FinnishWord.ToLower().Contains(lower) ||
                f.EnglishTranslation.ToLower().Contains(lower));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(f => f.Category!.Name)
            .ThenBy(f => f.FinnishWord)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Flashcard?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Flashcards
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<int> CountByCategoryAsync(int categoryId, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Flashcards.AsNoTracking()
            .Where(f => f.CategoryId == categoryId);

        if (excludeId.HasValue)
            query = query.Where(f => f.Id != excludeId.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Flashcard> CreateAsync(Flashcard entity, CancellationToken cancellationToken = default)
    {
        _context.Flashcards.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Flashcard entity, CancellationToken cancellationToken = default)
    {
        _context.Flashcards.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Flashcard entity, CancellationToken cancellationToken = default)
    {
        _context.Flashcards.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
