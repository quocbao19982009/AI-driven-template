using Backend.Common.Models;
using Backend.Features.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Backend.Tests.Features.Auth;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _serviceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _serviceMock = new Mock<IAuthService>();
        _sut = new AuthController(_serviceMock.Object);

        // Set up a default HttpContext with response cookies
        var httpContext = new DefaultHttpContext();
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    // --- Login ---

    [Fact]
    public async Task Login_ValidRequest_ReturnsOkWithAccessToken()
    {
        var request = new LoginRequest { Email = "alice@example.com", Password = "password" };
        var loginResult = new LoginResultDto("jwt-token") { RawRefreshToken = "raw-refresh" };

        _serviceMock
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        var result = await _sut.Login(request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<LoginResultDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.AccessToken.Should().Be("jwt-token");
    }

    // --- Refresh ---

    [Fact]
    public async Task Refresh_WithNoCookie_ReturnsUnauthorized()
    {
        // No cookie set — Request.Cookies["refreshToken"] is null
        var result = await _sut.Refresh();

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithValidCookie_ReturnsOkWithNewToken()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = "refreshToken=valid-raw-token";
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var loginResult = new LoginResultDto("new-jwt") { RawRefreshToken = "new-raw-refresh" };
        _serviceMock
            .Setup(s => s.RefreshAsync("valid-raw-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        var result = await _sut.Refresh();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<LoginResultDto>>().Subject;
        response.Data!.AccessToken.Should().Be("new-jwt");
    }

    // --- Logout ---

    [Fact]
    public async Task Logout_WithNoCookie_ReturnsNoContent()
    {
        var result = await _sut.Logout();

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.LogoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Logout_WithCookie_RevokesTokenAndReturnsNoContent()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = "refreshToken=some-token";
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        _serviceMock
            .Setup(s => s.LogoutAsync("some-token", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Logout();

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.LogoutAsync("some-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Me ---

    [Fact]
    public async Task Me_AuthenticatedUser_ReturnsOkWithProfile()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new("email", "alice@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var meDto = new MeDto(1, "Alice", "Smith", "alice@example.com", "User");
        _serviceMock
            .Setup(s => s.GetMeAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meDto);

        var result = await _sut.Me();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<MeDto>>().Subject;
        response.Data!.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task Me_MissingUserIdClaim_ReturnsUnauthorized()
    {
        // No claims set
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await _sut.Me();

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
