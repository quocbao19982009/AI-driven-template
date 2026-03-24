using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;
using Backend.Features.Rooms;

namespace Backend.Features.Locations;

public class Location : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
