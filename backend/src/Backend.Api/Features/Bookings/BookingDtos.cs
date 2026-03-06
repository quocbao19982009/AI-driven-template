namespace Backend.Features.Bookings;

public record BookingDto(
    int Id,
    int RoomId,
    string RoomName,
    DateTime StartTime,
    DateTime EndTime,
    string BookedBy,
    string? Purpose,
    DateTime CreatedAt
);

public class CreateBookingRequest
{
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string BookedBy { get; set; } = string.Empty;
    public string? Purpose { get; set; }
}

public class UpdateBookingRequest
{
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string BookedBy { get; set; } = string.Empty;
    public string? Purpose { get; set; }
}

public class BookingsListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? RoomId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
