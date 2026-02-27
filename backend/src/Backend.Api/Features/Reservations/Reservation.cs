using System.ComponentModel.DataAnnotations;
using Backend.Common.Models;
using Backend.Features.Factories;
using Backend.Features.Personnel;

namespace Backend.Features.Reservations;

public class Reservation : BaseEntity
{
    public int? FactoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FactoryDisplayName { get; set; } = string.Empty;

    public Factory? Factory { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public ICollection<ReservationPerson> ReservationPersonnel { get; set; } = [];
}

public class ReservationPerson : BaseEntity
{
    public int ReservationId { get; set; }
    public int? PersonId { get; set; }

    [Required]
    [MaxLength(300)]
    public string PersonDisplayName { get; set; } = string.Empty;

    public Reservation Reservation { get; set; } = null!;
    public Person? Person { get; set; }
}
