namespace FreelancerAssignment.CQRS.Products.Commands.Delete;

public sealed record DeleteProductCommand(Guid UserId, Guid Id) : ICommand;

public sealed class DeleteProductCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteProductCommandHandler> logger) : ICommandHandler<DeleteProductCommand>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<DeleteProductCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await _unitOfWork.Products.FindAsync(cancellationToken, request.Id) is not { } product)
                return ProductErrors.ProductNotFound;

            if (product.CreatedById != request.UserId)
                return ProductErrors.UnauthorizedAccess;

            product.DeletedAt = DateTime.Now;

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.CommitChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting the product");
            return Result.Failure(Error.FromException("DeleteProductError", ex.Message));
        }
    }
}
