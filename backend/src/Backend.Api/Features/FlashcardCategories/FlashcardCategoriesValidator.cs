using FluentValidation;

namespace Backend.Features.FlashcardCategories;

public class CreateFlashcardCategoryRequestValidator : AbstractValidator<CreateFlashcardCategoryRequest>
{
    public CreateFlashcardCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}

public class UpdateFlashcardCategoryRequestValidator : AbstractValidator<UpdateFlashcardCategoryRequest>
{
    public UpdateFlashcardCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}
