using FluentValidation;

namespace Backend.Features.Personnel;

public class CreatePersonRequestValidator : AbstractValidator<CreatePersonRequest>
{
    public CreatePersonRequestValidator()
    {
        RuleFor(x => x.PersonalId)
            .NotEmpty().WithMessage("PersonalId is required.")
            .MaximumLength(100).WithMessage("PersonalId must not exceed 100 characters.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(200).WithMessage("FullName must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(300).WithMessage("Email must not exceed 300 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}

public class UpdatePersonRequestValidator : AbstractValidator<UpdatePersonRequest>
{
    public UpdatePersonRequestValidator()
    {
        RuleFor(x => x.PersonalId)
            .NotEmpty().WithMessage("PersonalId is required.")
            .MaximumLength(100).WithMessage("PersonalId must not exceed 100 characters.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(200).WithMessage("FullName must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(300).WithMessage("Email must not exceed 300 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}
