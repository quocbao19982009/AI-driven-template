using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Todos;

public class TodosRepository : ITodosRepository
{
    private readonly ApplicationDbContext _context;

    public TodosRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Todo> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Todos.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Todo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Todos.FindAsync([id], cancellationToken);
    }

    public async Task<Todo> CreateAsync(Todo entity, CancellationToken cancellationToken = default)
    {
        _context.Todos.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Todo entity, CancellationToken cancellationToken = default)
    {
        _context.Todos.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Todo entity, CancellationToken cancellationToken = default)
    {
        _context.Todos.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
