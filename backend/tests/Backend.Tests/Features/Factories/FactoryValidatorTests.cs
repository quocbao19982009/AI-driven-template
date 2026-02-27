using Backend.Features.Factories;
using FluentValidation.TestHelper;

namespace Backend.Tests.Features.Factories;

public class CreateFactoryRequestValidatorTests
{
    private readonly CreateFactoryRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new CreateFactoryRequest { Name = "Factory Alpha", TimeZone = "UTC" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NameAt1Char_Passes()
    {
        var request = new CreateFactoryRequest { Name = "A", TimeZone = "UTC" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_NameAt200Chars_Passes()
    {
        var request = new CreateFactoryRequest { Name = new string('A', 200), TimeZone = "UTC" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // --- Name ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullName_Fails(string? name)
    {
        var request = new CreateFactoryRequest { Name = name!, TimeZone = "UTC" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Validate_NameExceeds200Chars_Fails()
    {
        var request = new CreateFactoryRequest { Name = new string('A', 201), TimeZone = "UTC" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters.");
    }

    // --- TimeZone ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullTimeZone_Fails(string? timeZone)
    {
        var request = new CreateFactoryRequest { Name = "Factory", TimeZone = timeZone! };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.TimeZone)
            .WithErrorMessage("TimeZone is required.");
    }

    [Fact]
    public async Task Validate_TimeZoneExceeds100Chars_Fails()
    {
        var request = new CreateFactoryRequest { Name = "Factory", TimeZone = new string('Z', 101) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.TimeZone)
            .WithErrorMessage("TimeZone must not exceed 100 characters.");
    }

    [Fact]
    public async Task Validate_TimeZoneAt100Chars_Passes()
    {
        var request = new CreateFactoryRequest { Name = "Factory", TimeZone = new string('Z', 100) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.TimeZone);
    }
}

public class UpdateFactoryRequestValidatorTests
{
    private readonly UpdateFactoryRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new UpdateFactoryRequest { Name = "Factory Beta", TimeZone = "America/New_York" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullName_Fails(string? name)
    {
        var request = new UpdateFactoryRequest { Name = name!, TimeZone = "UTC" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Validate_NameExceeds200Chars_Fails()
    {
        var request = new UpdateFactoryRequest { Name = new string('A', 201), TimeZone = "UTC" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullTimeZone_Fails(string? timeZone)
    {
        var request = new UpdateFactoryRequest { Name = "Factory", TimeZone = timeZone! };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.TimeZone)
            .WithErrorMessage("TimeZone is required.");
    }

    [Fact]
    public async Task Validate_TimeZoneExceeds100Chars_Fails()
    {
        var request = new UpdateFactoryRequest { Name = "Factory", TimeZone = new string('Z', 101) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.TimeZone)
            .WithErrorMessage("TimeZone must not exceed 100 characters.");
    }
}
