using FluentValidation;

namespace Backend.Features.Bookings;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.RoomId)
            .GreaterThan(0).WithMessage("RoomId is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.");

        RuleFor(x => x.BookedBy)
            .NotEmpty().WithMessage("BookedBy is required.")
            .MaximumLength(200).WithMessage("BookedBy must not exceed 200 characters.");

        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);
    }
}

public class UpdateBookingRequestValidator : AbstractValidator<UpdateBookingRequest>
{
    public UpdateBookingRequestValidator()
    {
        RuleFor(x => x.RoomId)
            .GreaterThan(0).WithMessage("RoomId is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.");

        RuleFor(x => x.BookedBy)
            .NotEmpty().WithMessage("BookedBy is required.")
            .MaximumLength(200).WithMessage("BookedBy must not exceed 200 characters.");

        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);
    }
}
