using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Common.Models;
using Backend.Features.Bookings;
using Backend.Features.Locations;

namespace Backend.Features.Rooms;

public class Room : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public int LocationId { get; set; }

    [ForeignKey(nameof(LocationId))]
    public Location Location { get; set; } = null!;

    [MaxLength(500)]
    public string? Purpose { get; set; }

    public string? ImagePath { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
