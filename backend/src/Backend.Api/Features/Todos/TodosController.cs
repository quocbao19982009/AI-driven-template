using Backend.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Todos;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private readonly ITodosService _todosService;

    public TodosController(ITodosService todosService)
    {
        _todosService = todosService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TodoDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _todosService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<TodoDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TodoDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var item = await _todosService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TodoDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TodoDto>>> Create(CreateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _todosService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<TodoDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TodoDto>>> Update(int id, UpdateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _todosService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TodoDto>.Ok(item));
    }

    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<ApiResponse<TodoDto>>> Toggle(int id, CancellationToken cancellationToken = default)
    {
        var item = await _todosService.ToggleAsync(id, cancellationToken);
        return Ok(ApiResponse<TodoDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<NoContentResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _todosService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
