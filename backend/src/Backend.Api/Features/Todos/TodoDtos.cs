namespace Backend.Features.Todos;

public record TodoDto(
    int Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateTodoRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}
