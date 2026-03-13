using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;
using Backend.Features.Users;

namespace Backend.Features.ExpenseTrackers;

public class ExpenseTracker : BaseEntity
{
    [Required]
    [Range(0.01, 999999.99)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public int UserId { get; set; }

    public User User { get; set; } = null!;
}
