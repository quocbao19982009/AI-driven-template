namespace Backend.Features.Scheduling;

public record ReservationSummaryDto(
    int ReservationId,
    DateTime StartTime,
    DateTime EndTime,
    double DurationHours,
    string FactoryName
);

public record PersonScheduleDto(
    int? PersonId,
    string PersonName,
    List<ReservationSummaryDto> Reservations,
    double TotalHours
);

public record FactoryScheduleDto(
    int? FactoryId,
    string FactoryName,
    double TotalHours,
    int ReservationCount
);
