using FreelancerAssignment.DTOs.Products;

namespace FreelancerAssignment.CQRS.Products.Commands.Add;

public sealed record AddProductCommand(Guid UserId, ProductRequest Product) : ICommand<Guid>;

public sealed class AddProductCommandHandler(
    IUnitOfWork unitOfWork, 
    ILogger<AddProductCommandHandler> logger,
    IFileService fileService) : ICommandHandler<AddProductCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<AddProductCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<Guid>> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = request.Product.Adapt<Product>();
            product.ProductCode = Guid.CreateVersion7().ToString();

            if (request.Product.Image is not null)
            {
                var imageUrl = await fileService.UploadImageAsync(request.Product.Image, cancellationToken);
                product.Image = imageUrl.IsSuccess ? imageUrl.Value! : string.Empty;
            }

            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.CommitChangesAsync(cancellationToken);

            return Result.Success(product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product");
            return Result.Failure<Guid>(Error.FromException("AddProductError", ex.Message));
        }
    }
}
