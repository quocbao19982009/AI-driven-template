using Backend.Common.Exceptions;
using Backend.Common.Models;
using Backend.Features.Auth;
using Backend.Features.Users;
using Backend.Identity;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ValidationException = Backend.Common.Exceptions.ValidationException;

namespace Backend.Tests.Features.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly Mock<IRefreshTokensRepository> _refreshTokensRepositoryMock;
    private readonly Mock<IValidator<LoginRequest>> _loginValidatorMock;
    private readonly JwtService _jwtService;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _usersRepositoryMock = new Mock<IUsersRepository>();
        _refreshTokensRepositoryMock = new Mock<IRefreshTokensRepository>();
        _loginValidatorMock = new Mock<IValidator<LoginRequest>>();

        // Build a minimal in-memory configuration for JwtService
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-super-secret-key-that-is-long-enough-for-hmac-sha256",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:ExpiryMinutes"] = "15"
            })
            .Build();

        _jwtService = new JwtService(configuration);

        _sut = new AuthService(
            _usersRepositoryMock.Object,
            _refreshTokensRepositoryMock.Object,
            _jwtService,
            _loginValidatorMock.Object,
            NullLogger<AuthService>.Instance);
    }

    // --- LoginAsync ---

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAccessToken()
    {
        var password = "correct-password";
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        var user = MakeUser(hash);

        SetupValidLoginValidator();
        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _refreshTokensRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken t, CancellationToken _) => t);

        var result = await _sut.LoginAsync(new LoginRequest { Email = "alice@example.com", Password = password });

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RawRefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_Throws401()
    {
        var user = MakeUser(BCrypt.Net.BCrypt.HashPassword("correct", workFactor: 4));

        SetupValidLoginValidator();
        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = () => _sut.LoginAsync(new LoginRequest { Email = "alice@example.com", Password = "wrong" });

        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_Throws401()
    {
        SetupValidLoginValidator();
        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginRequest { Email = "nobody@example.com", Password = "any" });

        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithNullPasswordHash_Throws401()
    {
        var user = MakeUser(passwordHash: null);

        SetupValidLoginValidator();
        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = () => _sut.LoginAsync(new LoginRequest { Email = "alice@example.com", Password = "any" });

        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidRequest_ThrowsValidationException()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required.")
        };
        _loginValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var act = () => _sut.LoginAsync(new LoginRequest { Email = "", Password = "" });

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Email is required.");
    }

    // --- RefreshAsync ---

    [Fact]
    public async Task RefreshAsync_WithValidToken_RotatesTokenAndReturnsNewJwt()
    {
        var user = MakeUser(BCrypt.Net.BCrypt.HashPassword("pw", workFactor: 4));
        var rawToken = "valid-raw-token";
        var hash = HashToken(rawToken);

        var storedToken = new RefreshToken
        {
            Id = 1,
            Token = hash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = user
        };

        _refreshTokensRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _refreshTokensRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokensRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken t, CancellationToken _) => t);

        var result = await _sut.RefreshAsync(rawToken);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RawRefreshToken.Should().NotBeNullOrEmpty();
        storedToken.RevokedAt.Should().NotBeNull("old token should be revoked on rotation");
        _refreshTokensRepositoryMock.Verify(r => r.UpdateAsync(storedToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredToken_Throws401()
    {
        var rawToken = "expired-raw-token";
        var hash = HashToken(rawToken);

        var expiredToken = new RefreshToken
        {
            Token = hash,
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // expired
            User = MakeUser()
        };

        _refreshTokensRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var act = () => _sut.RefreshAsync(rawToken);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefreshAsync_WithRevokedToken_RevokesAllUserTokens()
    {
        var rawToken = "stolen-raw-token";
        var hash = HashToken(rawToken);
        var user = MakeUser();

        var revokedToken = new RefreshToken
        {
            Token = hash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = DateTime.UtcNow.AddHours(-1), // already revoked
            User = user
        };

        var activeTokens = new List<RefreshToken>
        {
            new() { Token = "other-hash", UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddDays(5) }
        };

        _refreshTokensRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);
        _refreshTokensRepositoryMock
            .Setup(r => r.GetActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTokens);
        _refreshTokensRepositoryMock
            .Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var act = () => _sut.RefreshAsync(rawToken);

        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Contain("suspicious activity");
        _refreshTokensRepositoryMock.Verify(
            r => r.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- LogoutAsync ---

    [Fact]
    public async Task LogoutAsync_RevokesRefreshToken()
    {
        var rawToken = "valid-token";
        var hash = HashToken(rawToken);

        var storedToken = new RefreshToken
        {
            Token = hash,
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = MakeUser()
        };

        _refreshTokensRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _refreshTokensRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.LogoutAsync(rawToken);

        storedToken.RevokedAt.Should().NotBeNull();
        _refreshTokensRepositoryMock.Verify(r => r.UpdateAsync(storedToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Helpers ---

    private static User MakeUser(string? passwordHash = "hash")
    {
        return new User
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            PasswordHash = passwordHash,
            Role = UserRole.User
        };
    }

    private void SetupValidLoginValidator()
    {
        _loginValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static string HashToken(string rawToken)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(rawToken);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
