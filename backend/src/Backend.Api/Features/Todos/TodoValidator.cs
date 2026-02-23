using FluentValidation;

namespace Backend.Features.Todos;

public class CreateTodoRequestValidator : AbstractValidator<CreateTodoRequest>
{
    public CreateTodoRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 2).WithMessage("Priority must be 0 (Low), 1 (Medium), or 2 (High).");

        RuleFor(x => x.DueDate)
            .GreaterThan(_ => DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);
    }
}

public class UpdateTodoRequestValidator : AbstractValidator<UpdateTodoRequest>
{
    public UpdateTodoRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 2).WithMessage("Priority must be 0 (Low), 1 (Medium), or 2 (High).");

        RuleFor(x => x.DueDate)
            .GreaterThan(_ => DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);
    }
}
