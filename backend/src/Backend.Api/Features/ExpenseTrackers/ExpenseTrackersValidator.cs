using FluentValidation;

namespace Backend.Features.ExpenseTrackers;

public class CreateExpenseTrackerRequestValidator : AbstractValidator<CreateExpenseTrackerRequest>
{
    private static readonly string[] AllowedCategories =
        ["Food", "Transport", "Utilities", "Entertainment", "Other"];

    public CreateExpenseTrackerRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.")
            .LessThanOrEqualTo(999999.99m).WithMessage("Amount must not exceed 999,999.99.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters.")
            .Must(c => AllowedCategories.Contains(c))
            .WithMessage("Category must be one of: Food, Transport, Utilities, Entertainment, Other.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");
    }
}

public class UpdateExpenseTrackerRequestValidator : AbstractValidator<UpdateExpenseTrackerRequest>
{
    private static readonly string[] AllowedCategories =
        ["Food", "Transport", "Utilities", "Entertainment", "Other"];

    public UpdateExpenseTrackerRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.")
            .LessThanOrEqualTo(999999.99m).WithMessage("Amount must not exceed 999,999.99.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters.")
            .Must(c => AllowedCategories.Contains(c))
            .WithMessage("Category must be one of: Food, Transport, Utilities, Entertainment, Other.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");
    }
}
