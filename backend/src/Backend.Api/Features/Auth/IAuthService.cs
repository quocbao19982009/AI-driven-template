namespace Backend.Features.Auth;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResultDto> RefreshAsync(string rawToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string rawToken, CancellationToken cancellationToken = default);
    Task<MeDto> GetMeAsync(int userId, CancellationToken cancellationToken = default);
}
