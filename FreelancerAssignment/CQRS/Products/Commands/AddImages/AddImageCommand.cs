using FreelancerAssignment.Abstractions;
using FreelancerAssignment.Abstractions.Messaging;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using FreelancerAssignment.Service;

namespace FreelancerAssignment.CQRS.Products.Commands.AddImages;

public sealed record AddImageCommand(Guid UserId, Guid ProductId, IFormFile Image) : ICommand<string>;

public sealed class AddImageCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<AddImageCommandHandler> logger,
    IFileService fileService
    ) : ICommandHandler<AddImageCommand, string>
{
    public async Task<Result<string>> Handle(AddImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await unitOfWork.Products.FindAsync(cancellationToken, request.ProductId) is not { } product)
                return ProductErrors.ProductNotFound;

            var imageUrl = await fileService.UploadImageAsync(request.Image, cancellationToken);

            if (imageUrl.IsFailure)
                return imageUrl.Error;

            product.Image = imageUrl.Value!;
            await unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await unitOfWork.CommitChangesAsync(cancellationToken);

            return imageUrl.Value!;
        }
        catch ( Exception ex )
        {
            logger.LogError(ex, "error while add image");
            return Error.FromException("error while add image", ex.Message);

        }
    }
}
