namespace Backend.Features.Todos;

public interface ITodosRepository
{
    Task<(List<Todo> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Todo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Todo> CreateAsync(Todo entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Todo entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Todo entity, CancellationToken cancellationToken = default);
}
