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

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.");
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

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.");
    }
}
