using Backend.Features.Users;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Backend.Tests.Features.Users;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- FirstName ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullFirstName_Fails(string? firstName)
    {
        var request = new CreateUserRequest
        {
            FirstName = firstName!, LastName = "Doe",
            Email = "john@example.com", Password = "Password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Fact]
    public async Task Validate_FirstNameExceeds100Chars_Fails()
    {
        var request = new CreateUserRequest
        {
            FirstName = new string('A', 101), LastName = "Doe",
            Email = "john@example.com", Password = "Password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name must not exceed 100 characters.");
    }

    // --- LastName ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullLastName_Fails(string? lastName)
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = lastName!,
            Email = "john@example.com", Password = "Password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Fact]
    public async Task Validate_LastNameExceeds100Chars_Fails()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = new string('A', 101),
            Email = "john@example.com", Password = "Password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name must not exceed 100 characters.");
    }

    // --- Email ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullEmail_Fails(string? email)
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = email!, Password = "Password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public async Task Validate_InvalidEmail_Fails()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = "not-an-email", Password = "Password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("A valid email address is required.");
    }

    [Fact]
    public async Task Validate_EmailExceeds256Chars_Fails()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = new string('a', 245) + "@example.com", Password = "Password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 256 characters.");
    }

    // --- Password ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullPassword_Fails(string? password)
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = "john@example.com", Password = password!
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public async Task Validate_PasswordTooShort_Fails()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = "john@example.com", Password = "Pass1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public async Task Validate_PasswordNoUppercase_Fails()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = "john@example.com", Password = "password1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public async Task Validate_PasswordNoLowercase_Fails()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = "john@example.com", Password = "PASSWORD1"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public async Task Validate_PasswordNoDigit_Fails()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = "john@example.com", Password = "Passwordd"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one digit.");
    }
}

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullFirstName_Fails(string? firstName)
    {
        var request = new UpdateUserRequest
        {
            FirstName = firstName!, LastName = "Doe", Email = "john@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Fact]
    public async Task Validate_FirstNameExceeds100Chars_Fails()
    {
        var request = new UpdateUserRequest
        {
            FirstName = new string('A', 101), LastName = "Doe", Email = "john@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name must not exceed 100 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullLastName_Fails(string? lastName)
    {
        var request = new UpdateUserRequest
        {
            FirstName = "John", LastName = lastName!, Email = "john@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Fact]
    public async Task Validate_LastNameExceeds100Chars_Fails()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "John", LastName = new string('A', 101), Email = "john@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name must not exceed 100 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullEmail_Fails(string? email)
    {
        var request = new UpdateUserRequest
        {
            FirstName = "John", LastName = "Doe", Email = email!
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public async Task Validate_InvalidEmail_Fails()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "John", LastName = "Doe", Email = "not-an-email"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("A valid email address is required.");
    }

    [Fact]
    public async Task Validate_EmailExceeds256Chars_Fails()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "John", LastName = "Doe",
            Email = new string('a', 245) + "@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 256 characters.");
    }
}
