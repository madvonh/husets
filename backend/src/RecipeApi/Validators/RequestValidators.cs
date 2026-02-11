using FluentValidation;
using RecipeApi.Models.DTOs;

namespace RecipeApi.Validators;

public class CreateRecipeRequestValidator : AbstractValidator<CreateRecipeRequest>
{
    public CreateRecipeRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.RawText)
            .NotEmpty().WithMessage("Recipe text is required")
            .MaximumLength(10000).WithMessage("Recipe text must not exceed 10,000 characters");

        RuleFor(x => x.ImageRef)
            .NotEmpty().WithMessage("Image reference is required")
            .MaximumLength(500).WithMessage("Image reference must not exceed 500 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("Recipe cannot have more than 20 tags");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag cannot be empty")
            .MinimumLength(2).WithMessage("Tag must be at least 2 characters")
            .MaximumLength(50).WithMessage("Tag must not exceed 50 characters")
            .Matches("^[a-z0-9\\-]+$").WithMessage("Tag must contain only lowercase letters, numbers, and hyphens")
            .When(x => x.Tags != null);
    }
}

public class AddTagRequestValidator : AbstractValidator<AddTagRequest>
{
    public AddTagRequestValidator()
    {
        RuleFor(x => x.Tag)
            .NotEmpty().WithMessage("Tag is required")
            .MinimumLength(2).WithMessage("Tag must be at least 2 characters")
            .MaximumLength(50).WithMessage("Tag must not exceed 50 characters");
    }
}
