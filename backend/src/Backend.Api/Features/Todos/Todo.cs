using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;

namespace Backend.Features.Todos;

public class Todo : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? DueDate { get; set; }

    /// <summary>0 = Low, 1 = Medium, 2 = High</summary>
    public int Priority { get; set; } = 1;
}
