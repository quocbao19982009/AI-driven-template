using System.Security.Cryptography;
using Backend.Common.Exceptions;
using Backend.Features.Users;
using Backend.Identity;
using FluentValidation;

namespace Backend.Features.Auth;

public class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password";
    private const string SuspiciousActivityMessage = "Session invalidated due to suspicious activity";
    private const int RefreshTokenExpiryDays = 7;

    private readonly IUsersRepository _usersRepository;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly JwtService _jwtService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsersRepository usersRepository,
        IRefreshTokensRepository refreshTokensRepository,
        JwtService jwtService,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokensRepository = refreshTokensRepository;
        _jwtService = jwtService;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new Common.Exceptions.ValidationException(
                [.. validation.Errors.Select(e => e.ErrorMessage)]);
        }

        var user = await _usersRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Guard: user not found — return generic 401
        if (user is null)
        {
            _logger.LogWarning("Login attempt for unknown email");
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        // Guard: OAuth-only account (PasswordHash is null)
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogWarning("Login attempt on OAuth-only account for user {UserId}", user.Id);
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        // Guard: wrong password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user {UserId}", user.Id);
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        // Guard: deactivated account
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt on deactivated account for user {UserId}", user.Id);
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<LoginResultDto> RefreshAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(rawToken);
        var storedToken = await _refreshTokensRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        // Token not found
        if (storedToken is null)
        {
            _logger.LogWarning("Refresh attempt with unknown token");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Reuse detection: token was already revoked — invalidate entire session
        if (storedToken.IsRevoked)
        {
            _logger.LogWarning("Reused refresh token detected for user {UserId} — revoking all sessions", storedToken.UserId);
            var allActive = await _refreshTokensRepository.GetActiveByUserIdAsync(storedToken.UserId, cancellationToken);
            foreach (var t in allActive)
            {
                t.RevokedAt = DateTime.UtcNow;
            }
            await _refreshTokensRepository.UpdateRangeAsync(allActive, cancellationToken);
            throw new UnauthorizedAccessException(SuspiciousActivityMessage);
        }

        // Token expired
        if (storedToken.IsExpired)
        {
            _logger.LogWarning("Expired refresh token for user {UserId}", storedToken.UserId);
            throw new UnauthorizedAccessException("Refresh token has expired");
        }

        // Rotate: revoke old token
        storedToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokensRepository.UpdateAsync(storedToken, cancellationToken);

        // Issue new tokens
        return await IssueTokensAsync(storedToken.User, cancellationToken);
    }

    public async Task LogoutAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(rawToken);
        var storedToken = await _refreshTokensRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked)
        {
            // Already revoked or unknown — nothing to do
            return;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokensRepository.UpdateAsync(storedToken, cancellationToken);
        _logger.LogInformation("User {UserId} logged out", storedToken.UserId);
    }

    public async Task<MeDto> GetMeAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _usersRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        return new MeDto(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString());
    }

    /// <summary>
    /// Shared token issuance method — used by LoginAsync and future OAuth callbacks.
    /// Generates a JWT access token and a new refresh token, persists the refresh token.
    /// </summary>
    internal async Task<LoginResultDto> IssueTokensAsync(Users.User user, CancellationToken cancellationToken = default)
    {
        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        var rawRefreshToken = GenerateRawRefreshToken();
        var refreshTokenHash = HashToken(rawRefreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
        };

        await _refreshTokensRepository.CreateAsync(refreshTokenEntity, cancellationToken);

        // Return the raw token in LoginResultDto so the controller can set the HttpOnly cookie.
        // RawRefreshToken is marked [JsonIgnore] so it is never serialized into the response body.
        return new LoginResultDto(accessToken) { RawRefreshToken = rawRefreshToken };
    }

    private static string GenerateRawRefreshToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    /// <summary>
    /// SHA256 hash of the raw refresh token. SHA256 is appropriate here because
    /// the raw token is a cryptographically random 32-byte value (not a password).
    /// </summary>
    private static string HashToken(string rawToken)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
