using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;
using Backend.Features.Users;

namespace Backend.Features.Auth;

public class RefreshToken : BaseEntity
{
    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    [MaxLength(512)]
    public string? ReplacedByToken { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
}
