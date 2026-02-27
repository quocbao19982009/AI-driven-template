using Backend.Features.Personnel;
using FluentValidation.TestHelper;

namespace Backend.Tests.Features.Personnel;

public class CreatePersonRequestValidatorTests
{
    private readonly CreatePersonRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "EMP-001",
            FullName = "Alice Smith",
            Email = "alice@example.com",
            AllowedFactoryIds = []
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- PersonalId ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullPersonalId_Fails(string? personalId)
    {
        var request = new CreatePersonRequest
        {
            PersonalId = personalId!, FullName = "Alice Smith", Email = "alice@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PersonalId)
            .WithErrorMessage("PersonalId is required.");
    }

    [Fact]
    public async Task Validate_PersonalIdExceeds100Chars_Fails()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = new string('X', 101), FullName = "Alice Smith", Email = "alice@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PersonalId)
            .WithErrorMessage("PersonalId must not exceed 100 characters.");
    }

    [Fact]
    public async Task Validate_PersonalIdAt100Chars_Passes()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = new string('X', 100), FullName = "Alice Smith", Email = "alice@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PersonalId);
    }

    // --- FullName ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullFullName_Fails(string? fullName)
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = fullName!, Email = "alice@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("FullName is required.");
    }

    [Fact]
    public async Task Validate_FullNameExceeds200Chars_Fails()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = new string('A', 201), Email = "alice@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("FullName must not exceed 200 characters.");
    }

    // --- Email ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullEmail_Fails(string? email)
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith", Email = email!
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public async Task Validate_InvalidEmailFormat_Fails()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith", Email = "not-an-email"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be a valid email address.");
    }

    [Fact]
    public async Task Validate_EmailExceeds300Chars_Fails()
    {
        var localPart = new string('a', 289);
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith",
            Email = $"{localPart}@example.com" // 301 chars total
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 300 characters.");
    }

    [Fact]
    public async Task Validate_ValidEmail_Passes()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith", Email = "alice@company.org"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}

public class UpdatePersonRequestValidatorTests
{
    private readonly UpdatePersonRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = "EMP-001",
            FullName = "Bob Jones",
            Email = "bob@example.com",
            AllowedFactoryIds = []
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullPersonalId_Fails(string? personalId)
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = personalId!, FullName = "Bob Jones", Email = "bob@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PersonalId)
            .WithErrorMessage("PersonalId is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullFullName_Fails(string? fullName)
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = "P001", FullName = fullName!, Email = "bob@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("FullName is required.");
    }

    [Fact]
    public async Task Validate_InvalidEmailFormat_Fails()
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = "P001", FullName = "Bob Jones", Email = "bad-email"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be a valid email address.");
    }

    [Fact]
    public async Task Validate_PersonalIdExceeds100Chars_Fails()
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = new string('X', 101), FullName = "Bob Jones", Email = "bob@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PersonalId)
            .WithErrorMessage("PersonalId must not exceed 100 characters.");
    }

    [Fact]
    public async Task Validate_FullNameExceeds200Chars_Fails()
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = "P001", FullName = new string('B', 201), Email = "bob@example.com"
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("FullName must not exceed 200 characters.");
    }
}
