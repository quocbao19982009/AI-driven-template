using FluentValidation;

namespace Backend.Features.Rooms;

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be at least 1.");
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location is required.");
        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);
    }
}

public class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be at least 1.");
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location is required.");
        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);
    }
}
