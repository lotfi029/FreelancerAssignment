using FluentValidation;
using FreelancerAssignment.IRepositories;

namespace FreelancerAssignment.DTOs.Products;

public sealed class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductRequestValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(e => e.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(e => e.Category)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(e => e.Price)
            .GreaterThan(0);

        RuleFor(e => e.MinimumQuantity)
            .GreaterThan(0);

        RuleFor(e => e.Discount)
            .Must(d => d.HasValue && d.Value > 0 && d.Value <= 100 || !d.HasValue)
            .WithMessage("Discount must be between 0 and 100 if provided.")
            .When(e => e.Discount.HasValue);

        _unitOfWork = unitOfWork;
    }
}