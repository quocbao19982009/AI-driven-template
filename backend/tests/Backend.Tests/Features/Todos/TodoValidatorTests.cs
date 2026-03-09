using Backend.Features.Todos;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Backend.Tests.Features.Todos;

public class CreateTodoRequestValidatorTests
{
    private readonly CreateTodoRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidTitleOnly_Passes()
    {
        var request = new CreateTodoRequest { Title = "Buy milk" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_AllFieldsValid_Passes()
    {
        var request = new CreateTodoRequest
        {
            Title = "Submit report",
            Description = "Q1 report",
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullTitle_FailsWithMessage(string? title)
    {
        var request = new CreateTodoRequest { Title = title! };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public async Task Validate_TitleExceeds200Chars_FailsWithMessage()
    {
        var request = new CreateTodoRequest { Title = new string('A', 201) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_TitleExactly200Chars_Passes()
    {
        var request = new CreateTodoRequest { Title = new string('A', 200) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_DescriptionExceeds1000Chars_FailsWithMessage()
    {
        var request = new CreateTodoRequest { Title = "Valid", Description = new string('A', 1001) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    [Fact]
    public async Task Validate_DescriptionNull_Passes()
    {
        var request = new CreateTodoRequest { Title = "Valid", Description = null };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_PastDueDate_FailsWithMessage()
    {
        var request = new CreateTodoRequest
        {
            Title = "Old task",
            DueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("DueDate must be in the future.");
    }

    [Fact]
    public async Task Validate_NullDueDate_Passes()
    {
        var request = new CreateTodoRequest { Title = "Valid", DueDate = null };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_FutureDueDate_Passes()
    {
        var request = new CreateTodoRequest { Title = "Valid", DueDate = DateTime.UtcNow.AddDays(1) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateTodoRequestValidatorTests
{
    private readonly UpdateTodoRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new UpdateTodoRequest { Title = "Updated title" };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullTitle_FailsWithMessage(string? title)
    {
        var request = new UpdateTodoRequest { Title = title! };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public async Task Validate_TitleExceeds200Chars_FailsWithMessage()
    {
        var request = new UpdateTodoRequest { Title = new string('A', 201) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_DescriptionExceeds1000Chars_FailsWithMessage()
    {
        var request = new UpdateTodoRequest { Title = "Valid", Description = new string('A', 1001) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    [Fact]
    public async Task Validate_PastDueDate_FailsWithMessage()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Valid",
            DueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("DueDate must be in the future.");
    }
}
