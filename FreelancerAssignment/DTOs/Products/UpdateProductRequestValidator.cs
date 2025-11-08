namespace FreelancerAssignment.DTOs.Products;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.");
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
        RuleFor(x => x.MinimumQuantity)
            .GreaterThan(0).WithMessage("Minimum quantity must be greater than zero.");
        RuleFor(x => x.Discount)
            .InclusiveBetween(0, 100).When(x => x.Discount.HasValue)
            .WithMessage("Discount must be between 0 and 100.");
    }
}