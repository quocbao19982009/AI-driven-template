using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;
using Backend.Features.Locations;

namespace Backend.Features.Rooms;

public class Room : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    [MaxLength(500)]
    public string? Purpose { get; set; }

    [MaxLength(500)]
    public string? ImagePath { get; set; }

    public ICollection<Bookings.Booking> Bookings { get; set; } = [];
}
