using FluentValidation;

namespace Backend.Features.Flashcards;

public class CreateFlashcardRequestValidator : AbstractValidator<CreateFlashcardRequest>
{
    public CreateFlashcardRequestValidator()
    {
        RuleFor(x => x.FinnishWord)
            .NotEmpty().WithMessage("Finnish word is required.")
            .MaximumLength(500).WithMessage("Finnish word must not exceed 500 characters.");

        RuleFor(x => x.EnglishTranslation)
            .NotEmpty().WithMessage("English translation is required.")
            .MaximumLength(500).WithMessage("English translation must not exceed 500 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.")
            .When(x => x.CategoryId.HasValue);
    }
}

public class UpdateFlashcardRequestValidator : AbstractValidator<UpdateFlashcardRequest>
{
    public UpdateFlashcardRequestValidator()
    {
        RuleFor(x => x.FinnishWord)
            .NotEmpty().WithMessage("Finnish word is required.")
            .MaximumLength(500).WithMessage("Finnish word must not exceed 500 characters.");

        RuleFor(x => x.EnglishTranslation)
            .NotEmpty().WithMessage("English translation is required.")
            .MaximumLength(500).WithMessage("English translation must not exceed 500 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.")
            .When(x => x.CategoryId.HasValue);
    }
}
