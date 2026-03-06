using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;

namespace Backend.Features.Locations;

public class Location : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Rooms.Room> Rooms { get; set; } = [];
}
