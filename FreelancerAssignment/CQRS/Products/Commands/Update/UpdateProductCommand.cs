using FreelancerAssignment.DTOs.Products;

namespace FreelancerAssignment.CQRS.Products.Commands.Update;

public sealed record UpdateProductCommand(Guid UserId, Guid Id, UpdateProductRequest Product) : ICommand;

public sealed class UpdateProductCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<UpdateProductCommandHandler> logger) : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await unitOfWork.Products.FindAsync(cancellationToken, request.Id) is not { } product)
                return ProductErrors.ProductNotFound;

            if (product.CreatedById != request.UserId)
                return ProductErrors.UnauthorizedAccess;

            product = request.Product.Adapt(product);

            await unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await unitOfWork.CommitChangesAsync(cancellationToken);
            return Result.Success();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating the product");
            return Error.FromException("UpdateProductError", ex.Message);
        }
    }
}