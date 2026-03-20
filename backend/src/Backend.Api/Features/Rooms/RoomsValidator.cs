using FluentValidation;

namespace Backend.Features.Rooms;

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("LocationId is required.");

        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);

        When(x => x.Image is not null, () =>
        {
            RuleFor(x => x.Image!.ContentType)
                .Must(ct => AllowedMimeTypes.Contains(ct))
                .WithMessage("Image must be a JPEG or PNG file.");

            RuleFor(x => x.Image!.Length)
                .LessThanOrEqualTo(MaxImageSizeBytes)
                .WithMessage("Image must not exceed 5 MB.");
        });
    }
}

public class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("LocationId is required.");

        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);

        When(x => x.Image is not null, () =>
        {
            RuleFor(x => x.Image!.ContentType)
                .Must(ct => AllowedMimeTypes.Contains(ct))
                .WithMessage("Image must be a JPEG or PNG file.");

            RuleFor(x => x.Image!.Length)
                .LessThanOrEqualTo(MaxImageSizeBytes)
                .WithMessage("Image must not exceed 5 MB.");
        });
    }
}
