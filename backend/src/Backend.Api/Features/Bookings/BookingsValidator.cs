using FluentValidation;

namespace Backend.Features.Bookings;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("Room is required.");
        RuleFor(x => x.StartTime).NotEmpty().WithMessage("Start time is required.");
        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
        RuleFor(x => x.BookedBy)
            .NotEmpty().WithMessage("Booked by is required.")
            .MaximumLength(200).WithMessage("Booked by must not exceed 200 characters.");
        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);
    }
}

public class UpdateBookingRequestValidator : AbstractValidator<UpdateBookingRequest>
{
    public UpdateBookingRequestValidator()
    {
        RuleFor(x => x.RoomId).GreaterThan(0).WithMessage("Room is required.");
        RuleFor(x => x.StartTime).NotEmpty().WithMessage("Start time is required.");
        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
        RuleFor(x => x.BookedBy)
            .NotEmpty().WithMessage("Booked by is required.")
            .MaximumLength(200).WithMessage("Booked by must not exceed 200 characters.");
        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);
    }
}
