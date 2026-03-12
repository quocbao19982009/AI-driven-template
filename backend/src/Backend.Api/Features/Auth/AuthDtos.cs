namespace Backend.Features.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Returned from the service after successful authentication.
/// RawRefreshToken is an internal field used to pass the plaintext token
/// to the controller so it can set the HttpOnly cookie — it is never
/// serialized in the API response body.
/// </summary>
public class LoginResultDto
{
    public LoginResultDto(string accessToken)
    {
        AccessToken = accessToken;
    }

    public string AccessToken { get; init; }

    // Internal only — not serialized. Controller reads this to set the cookie.
    [System.Text.Json.Serialization.JsonIgnore]
    public string RawRefreshToken { get; set; } = string.Empty;
}

public record MeDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role
);
