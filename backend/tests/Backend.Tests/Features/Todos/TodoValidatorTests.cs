using Backend.Features.Todos;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Backend.Tests.Features.Todos;

public class CreateTodoRequestValidatorTests
{
    private readonly CreateTodoRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new CreateTodoRequest
        {
            Title = "Buy groceries",
            Description = "Milk and eggs",
            Priority = 1,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_MinimalValidRequest_Passes()
    {
        var request = new CreateTodoRequest { Title = "A", Priority = 0 };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- Title ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullTitle_Fails(string? title)
    {
        var request = new CreateTodoRequest { Title = title!, Priority = 1 };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public async Task Validate_TitleExceeds200Chars_Fails()
    {
        var request = new CreateTodoRequest { Title = new string('A', 201), Priority = 1 };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_TitleAt200Chars_Passes()
    {
        var request = new CreateTodoRequest { Title = new string('A', 200), Priority = 1 };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    // --- Description ---

    [Fact]
    public async Task Validate_NullDescription_Passes()
    {
        var request = new CreateTodoRequest { Title = "Title", Description = null, Priority = 1 };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_DescriptionExceeds1000Chars_Fails()
    {
        var request = new CreateTodoRequest
        {
            Title = "Title",
            Description = new string('A', 1001),
            Priority = 1
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    // --- Priority ---

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Validate_ValidPriority_Passes(int priority)
    {
        var request = new CreateTodoRequest { Title = "Title", Priority = priority };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public async Task Validate_InvalidPriority_Fails(int priority)
    {
        var request = new CreateTodoRequest { Title = "Title", Priority = priority };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Priority)
            .WithErrorMessage("Priority must be 0 (Low), 1 (Medium), or 2 (High).");
    }

    // --- DueDate ---

    [Fact]
    public async Task Validate_NullDueDate_Passes()
    {
        var request = new CreateTodoRequest { Title = "Title", Priority = 1, DueDate = null };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_FutureDueDate_Passes()
    {
        var request = new CreateTodoRequest { Title = "Title", Priority = 1, DueDate = DateTime.UtcNow.AddDays(1) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async Task Validate_PastDueDate_Fails()
    {
        var request = new CreateTodoRequest { Title = "Title", Priority = 1, DueDate = DateTime.UtcNow.AddDays(-1) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("Due date must be in the future.");
    }
}

public class UpdateTodoRequestValidatorTests
{
    private readonly UpdateTodoRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Updated title",
            Priority = 2,
            IsCompleted = true
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullTitle_Fails(string? title)
    {
        var request = new UpdateTodoRequest { Title = title!, Priority = 1, IsCompleted = false };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public async Task Validate_TitleExceeds200Chars_Fails()
    {
        var request = new UpdateTodoRequest { Title = new string('A', 201), Priority = 1, IsCompleted = false };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public async Task Validate_InvalidPriority_Fails(int priority)
    {
        var request = new UpdateTodoRequest { Title = "Title", Priority = priority, IsCompleted = false };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Priority)
            .WithErrorMessage("Priority must be 0 (Low), 1 (Medium), or 2 (High).");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_AnyIsCompletedValue_Passes(bool isCompleted)
    {
        var request = new UpdateTodoRequest { Title = "Title", Priority = 1, IsCompleted = isCompleted };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.IsCompleted);
    }

    [Fact]
    public async Task Validate_PastDueDate_Fails()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Title", Priority = 1, IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("Due date must be in the future.");
    }
}
