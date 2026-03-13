namespace Backend.Features.ExpenseTrackers;

public record ExpenseTrackerDto(
    int Id,
    decimal Amount,
    string Category,
    string? Description,
    DateTime Date,
    int UserId,
    string SubmittedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateExpenseTrackerRequest
{
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
}

public class UpdateExpenseTrackerRequest
{
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
}
