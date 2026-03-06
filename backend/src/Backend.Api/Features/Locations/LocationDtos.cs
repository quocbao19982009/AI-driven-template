namespace Backend.Features.Locations;

public record LocationDto(
    int Id,
    string Name,
    DateTime CreatedAt
);

public class CreateLocationRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateLocationRequest
{
    public string Name { get; set; } = string.Empty;
}
