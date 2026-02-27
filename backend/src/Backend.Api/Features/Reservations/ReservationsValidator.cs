using FluentValidation;

namespace Backend.Features.Reservations;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.FactoryId)
            .GreaterThan(0).WithMessage("FactoryId is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.")
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime.");

        RuleFor(x => x.PersonIds)
            .NotEmpty().WithMessage("At least one person is required.");
    }
}

public class UpdateReservationRequestValidator : AbstractValidator<UpdateReservationRequest>
{
    public UpdateReservationRequestValidator()
    {
        RuleFor(x => x.FactoryId)
            .GreaterThan(0).WithMessage("FactoryId is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.")
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime.");

        RuleFor(x => x.PersonIds)
            .NotEmpty().WithMessage("At least one person is required.");
    }
}
