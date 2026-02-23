using FluentValidation;

namespace Backend.Features._FeatureTemplate;

// TODO: Rename to match your entity (e.g., CreateProductRequestValidator)

public class CreateFeatureRequestValidator : AbstractValidator<CreateFeatureRequest>
{
    public CreateFeatureRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        // TODO: Add validation rules for your properties
    }
}

public class UpdateFeatureRequestValidator : AbstractValidator<UpdateFeatureRequest>
{
    public UpdateFeatureRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        // TODO: Add validation rules for your properties
    }
}
