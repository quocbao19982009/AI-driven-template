using Backend.Features._FeatureTemplate;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Backend.Tests.Features._FeatureTemplate;

public class CreateFeatureRequestValidatorTests
{
    private readonly CreateFeatureRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidName_Passes()
    {
        var request = new CreateFeatureRequest { Name = "Valid Name" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullName_FailsWithMessage(string? name)
    {
        var request = new CreateFeatureRequest { Name = name! };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Validate_NameExceeds200Chars_FailsWithMessage()
    {
        var request = new CreateFeatureRequest { Name = new string('A', 201) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_NameExactly200Chars_Passes()
    {
        var request = new CreateFeatureRequest { Name = new string('A', 200) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateFeatureRequestValidatorTests
{
    private readonly UpdateFeatureRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidName_Passes()
    {
        var request = new UpdateFeatureRequest { Name = "Valid Name" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullName_FailsWithMessage(string? name)
    {
        var request = new UpdateFeatureRequest { Name = name! };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Validate_NameExceeds200Chars_FailsWithMessage()
    {
        var request = new UpdateFeatureRequest { Name = new string('A', 201) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters.");
    }
}
