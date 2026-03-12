using Backend.Features.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Backend.Tests.Features.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    // --- Email ---

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new LoginRequest { Email = "user@example.com", Password = "secret123" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyEmail_FailsWithMessage(string? email)
    {
        var request = new LoginRequest { Email = email!, Password = "secret123" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public async Task Validate_InvalidEmailFormat_FailsWithMessage()
    {
        var request = new LoginRequest { Email = "not-an-email", Password = "secret123" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("A valid email address is required.");
    }

    [Fact]
    public async Task Validate_EmailExceeds256Chars_FailsWithMessage()
    {
        var request = new LoginRequest
        {
            Email = new string('a', 251) + "@b.com",
            Password = "secret123"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 256 characters.");
    }

    // --- Password ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyPassword_FailsWithMessage(string? password)
    {
        var request = new LoginRequest { Email = "user@example.com", Password = password! };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public async Task Validate_PasswordExceeds200Chars_FailsWithMessage()
    {
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = new string('A', 201)
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_PasswordExactly200Chars_Passes()
    {
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = new string('A', 200)
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
