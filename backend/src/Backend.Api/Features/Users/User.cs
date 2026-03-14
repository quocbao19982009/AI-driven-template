using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;

namespace Backend.Features.Users;

public class User : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    // Nullable to support OAuth-only users who never set a password.
    // Local login must reject with 401 if this is null.
    public string? PasswordHash { get; set; }

    [MaxLength(50)]
    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;
}
