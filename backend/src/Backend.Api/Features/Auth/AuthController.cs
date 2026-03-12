using System.Security.Claims;
using Backend.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";
    private static readonly CookieOptions RefreshTokenCookieOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/api/auth"
    };

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticate with email and password. Returns a JWT access token in the body
    /// and sets a HttpOnly refresh token cookie.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        SetRefreshTokenCookie(result.RawRefreshToken);
        return Ok(ApiResponse<LoginResultDto>.Ok(result));
    }

    /// <summary>
    /// Issue a new JWT access token using the HttpOnly refresh token cookie.
    /// Performs refresh token rotation.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Refresh(
        CancellationToken cancellationToken = default)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(rawToken))
        {
            return Unauthorized(ApiResponse<LoginResultDto>.Fail("No refresh token provided"));
        }

        var result = await _authService.RefreshAsync(rawToken, cancellationToken);
        SetRefreshTokenCookie(result.RawRefreshToken);
        return Ok(ApiResponse<LoginResultDto>.Ok(result));
    }

    /// <summary>
    /// Revoke the refresh token and clear the cookie. Returns 204 No Content.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrEmpty(rawToken))
        {
            await _authService.LogoutAsync(rawToken, cancellationToken);
        }

        ClearRefreshTokenCookie();
        return NoContent();
    }

    /// <summary>
    /// Return the current authenticated user's profile. Requires a valid JWT bearer token.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<MeDto>>> Me(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<MeDto>.Fail("Invalid token claims"));
        }

        var me = await _authService.GetMeAsync(userId, cancellationToken);
        return Ok(ApiResponse<MeDto>.Ok(me));
    }

    private void SetRefreshTokenCookie(string rawToken)
    {
        var options = new CookieOptions
        {
            HttpOnly = RefreshTokenCookieOptions.HttpOnly,
            Secure = RefreshTokenCookieOptions.Secure,
            SameSite = RefreshTokenCookieOptions.SameSite,
            Path = RefreshTokenCookieOptions.Path,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };
        Response.Cookies.Append(RefreshTokenCookieName, rawToken, options);
    }

    private void ClearRefreshTokenCookie()
    {
        var options = new CookieOptions
        {
            HttpOnly = RefreshTokenCookieOptions.HttpOnly,
            Secure = RefreshTokenCookieOptions.Secure,
            SameSite = RefreshTokenCookieOptions.SameSite,
            Path = RefreshTokenCookieOptions.Path,
            Expires = DateTimeOffset.UnixEpoch
        };
        Response.Cookies.Append(RefreshTokenCookieName, string.Empty, options);
    }
}
