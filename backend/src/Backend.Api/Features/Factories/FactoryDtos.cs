namespace Backend.Features.Factories;

public record FactoryDto(
    int Id,
    string Name,
    string TimeZone,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateFactoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}

public class UpdateFactoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}
