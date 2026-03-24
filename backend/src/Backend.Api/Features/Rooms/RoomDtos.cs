using Microsoft.AspNetCore.Http;

namespace Backend.Features.Rooms;

public record RoomDto(
    int Id,
    string Name,
    int Capacity,
    int LocationId,
    string LocationName,
    string? Purpose,
    string? ImagePath,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int LocationId { get; set; }
    public string? Purpose { get; set; }
    public IFormFile? Image { get; set; }
}

public class UpdateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int LocationId { get; set; }
    public string? Purpose { get; set; }
    public IFormFile? Image { get; set; }
}

public class GetRoomsQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public int? LocationId { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDir { get; set; } = "asc";
}
