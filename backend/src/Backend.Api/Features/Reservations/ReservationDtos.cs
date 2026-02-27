namespace Backend.Features.Reservations;

public record ReservationPersonDto(
    int Id,
    int? PersonId,
    string PersonDisplayName
);

public record ReservationDto(
    int Id,
    int? FactoryId,
    string FactoryDisplayName,
    DateTime StartTime,
    DateTime EndTime,
    double DurationHours,
    List<ReservationPersonDto> Personnel,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateReservationRequest
{
    public int FactoryId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<int> PersonIds { get; set; } = [];
}

public class UpdateReservationRequest
{
    public int FactoryId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<int> PersonIds { get; set; } = [];
}
