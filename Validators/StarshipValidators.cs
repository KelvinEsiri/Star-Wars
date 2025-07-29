using FluentValidation;
using Star_Wars.DTOs;

namespace Star_Wars.Validators
{
    public class CreateStarshipDtoValidator : AbstractValidator<CreateStarshipDto>
    {
        public CreateStarshipDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Model)
                .MaximumLength(50).WithMessage("Model cannot exceed 50 characters");

            RuleFor(x => x.Manufacturer)
                .MaximumLength(50).WithMessage("Manufacturer cannot exceed 50 characters");

            RuleFor(x => x.StarshipClass)
                .MaximumLength(50).WithMessage("Starship class cannot exceed 50 characters");
        }
    }

    public class UpdateStarshipDtoValidator : AbstractValidator<UpdateStarshipDto>
    {
        public UpdateStarshipDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Model)
                .MaximumLength(50).WithMessage("Model cannot exceed 50 characters");

            RuleFor(x => x.Manufacturer)
                .MaximumLength(50).WithMessage("Manufacturer cannot exceed 50 characters");

            RuleFor(x => x.StarshipClass)
                .MaximumLength(50).WithMessage("Starship class cannot exceed 50 characters");
        }
    }

    public class StarshipQueryDtoValidator : AbstractValidator<StarshipQueryDto>
    {
        public StarshipQueryDtoValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            RuleFor(x => x.SortBy)
                .Must(BeValidSortField).WithMessage("Invalid sort field")
                .When(x => !string.IsNullOrEmpty(x.SortBy));
        }

        private bool BeValidSortField(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
                return true;

            var validFields = new[] { "name", "model", "manufacturer", "starshipclass", "created" };
            return validFields.Contains(sortBy.ToLower());
        }
    }
}
