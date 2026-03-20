using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Common.Models;
using Backend.Features.Rooms;

namespace Backend.Features.Bookings;

public class Booking : BaseEntity
{
    public int RoomId { get; set; }

    [ForeignKey(nameof(RoomId))]
    public Room Room { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    [Required]
    [MaxLength(200)]
    public string BookedBy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Purpose { get; set; }
}
