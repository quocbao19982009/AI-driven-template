using Backend.Features.Factories;

namespace Backend.Features.Personnel;

public record PersonDto(
    int Id,
    string PersonalId,
    string FullName,
    string Email,
    List<FactoryDto> AllowedFactories,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreatePersonRequest
{
    public string PersonalId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<int> AllowedFactoryIds { get; set; } = [];
}

public class UpdatePersonRequest
{
    public string PersonalId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<int> AllowedFactoryIds { get; set; } = [];
}
