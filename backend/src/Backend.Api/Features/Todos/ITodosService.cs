using Backend.Common.Models;

namespace Backend.Features.Todos;

public interface ITodosService
{
    Task<PagedResult<TodoDto>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        bool? isCompleted,
        int? priority,
        CancellationToken cancellationToken = default);

    Task<TodoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoDto> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default);
    Task<TodoDto> UpdateAsync(int id, UpdateTodoRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoDto> ToggleAsync(int id, CancellationToken cancellationToken = default);
}
