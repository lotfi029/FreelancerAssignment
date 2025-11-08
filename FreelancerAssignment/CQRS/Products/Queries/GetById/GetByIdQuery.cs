using FreelancerAssignment.DTOs.Products;

namespace FreelancerAssignment.CQRS.Products.Queries.GetById;

public sealed record GetByIdQuery(Guid Id) : IQuery<ProductResponse>;

public sealed class GetByIdQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetByIdQueryHandler> logger,
    IUrlGenratorService urlGenratorService) : IQueryHandler<GetByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (await unitOfWork.Products.FindAsync(cancellationToken, request.Id, cancellationToken) is not { } product)
                return ProductErrors.ProductNotFound;

            product.Image = urlGenratorService.GetImageUrl(product.Id) ?? string.Empty;

            var response = product.Adapt<ProductResponse>();

            return response;

        }
        catch(Exception ex)
        {
            logger.LogError(ex, "error while retrive the product");
            return Error.FromException("error while retrive the product", ex.Message);

        }
    }
}
