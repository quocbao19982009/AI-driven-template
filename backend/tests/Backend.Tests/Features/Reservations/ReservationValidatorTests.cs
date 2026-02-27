using Backend.Features.Reservations;
using FluentValidation.TestHelper;

namespace Backend.Tests.Features.Reservations;

public class CreateReservationRequestValidatorTests
{
    private readonly CreateReservationRequestValidator _validator = new();

    private static readonly DateTime ValidStart = new(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ValidEnd = new(2026, 6, 1, 16, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1,
            StartTime = ValidStart,
            EndTime = ValidEnd,
            PersonIds = [1, 2]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- FactoryId ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_FactoryIdZeroOrNegative_Fails(int factoryId)
    {
        var request = new CreateReservationRequest
        {
            FactoryId = factoryId, StartTime = ValidStart, EndTime = ValidEnd, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FactoryId)
            .WithErrorMessage("FactoryId is required.");
    }

    [Fact]
    public async Task Validate_FactoryIdPositive_Passes()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = ValidStart, EndTime = ValidEnd, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.FactoryId);
    }

    // --- StartTime ---

    [Fact]
    public async Task Validate_DefaultStartTime_Fails()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = default, EndTime = ValidEnd, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("StartTime is required.");
    }

    // --- EndTime ---

    [Fact]
    public async Task Validate_EndTimeBeforeStartTime_Fails()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = ValidEnd, EndTime = ValidStart, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime must be after StartTime.");
    }

    [Fact]
    public async Task Validate_EndTimeEqualToStartTime_Fails()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = ValidStart, EndTime = ValidStart, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime must be after StartTime.");
    }

    [Fact]
    public async Task Validate_EndTimeAfterStartTime_Passes()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = ValidStart, EndTime = ValidEnd, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    // --- PersonIds ---

    [Fact]
    public async Task Validate_EmptyPersonIds_Fails()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = ValidStart, EndTime = ValidEnd, PersonIds = []
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PersonIds)
            .WithErrorMessage("At least one person is required.");
    }

    [Fact]
    public async Task Validate_SinglePersonId_Passes()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = ValidStart, EndTime = ValidEnd, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PersonIds);
    }
}

public class UpdateReservationRequestValidatorTests
{
    private readonly UpdateReservationRequestValidator _validator = new();

    private static readonly DateTime ValidStart = new(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ValidEnd = new(2026, 6, 1, 16, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var request = new UpdateReservationRequest
        {
            FactoryId = 2,
            StartTime = ValidStart,
            EndTime = ValidEnd,
            PersonIds = [3]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Validate_FactoryIdZeroOrNegative_Fails(int factoryId)
    {
        var request = new UpdateReservationRequest
        {
            FactoryId = factoryId, StartTime = ValidStart, EndTime = ValidEnd, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.FactoryId)
            .WithErrorMessage("FactoryId is required.");
    }

    [Fact]
    public async Task Validate_EndTimeBeforeStartTime_Fails()
    {
        var request = new UpdateReservationRequest
        {
            FactoryId = 1, StartTime = ValidEnd, EndTime = ValidStart, PersonIds = [1]
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("EndTime must be after StartTime.");
    }

    [Fact]
    public async Task Validate_EmptyPersonIds_Fails()
    {
        var request = new UpdateReservationRequest
        {
            FactoryId = 1, StartTime = ValidStart, EndTime = ValidEnd, PersonIds = []
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PersonIds)
            .WithErrorMessage("At least one person is required.");
    }
}
