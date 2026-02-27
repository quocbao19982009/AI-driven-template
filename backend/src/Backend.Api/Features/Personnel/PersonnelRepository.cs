using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Personnel;

public class PersonnelRepository : IPersonnelRepository
{
    private readonly ApplicationDbContext _context;

    public PersonnelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Person> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Personnel
            .AsNoTracking()
            .Include(p => p.AllowedFactories);

        IQueryable<Person> filtered = query;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            filtered = filtered.Where(p =>
                p.FullName.ToLower().Contains(lower) ||
                p.PersonalId.ToLower().Contains(lower) ||
                p.Email.ToLower().Contains(lower));
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await filtered
            .OrderBy(p => p.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Person>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Personnel
            .AsNoTracking()
            .Include(p => p.AllowedFactories)
            .OrderBy(p => p.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Personnel
            .Include(p => p.AllowedFactories)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithPersonalIdAsync(string personalId, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Personnel.AsNoTracking()
            .Where(p => p.PersonalId.ToLower() == personalId.ToLower());

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Personnel.AsNoTracking()
            .Where(p => p.Email.ToLower() == email.ToLower());

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Person> CreateAsync(Person entity, CancellationToken cancellationToken = default)
    {
        _context.Personnel.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Person entity, CancellationToken cancellationToken = default)
    {
        _context.Personnel.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Person entity, CancellationToken cancellationToken = default)
    {
        _context.Personnel.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
