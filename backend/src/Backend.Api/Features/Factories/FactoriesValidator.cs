using FluentValidation;

namespace Backend.Features.Factories;

public class CreateFactoryRequestValidator : AbstractValidator<CreateFactoryRequest>
{
    public CreateFactoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required.")
            .MaximumLength(100).WithMessage("TimeZone must not exceed 100 characters.");
    }
}

public class UpdateFactoryRequestValidator : AbstractValidator<UpdateFactoryRequest>
{
    public UpdateFactoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required.")
            .MaximumLength(100).WithMessage("TimeZone must not exceed 100 characters.");
    }
}
