using FreelancerAssignment.DTOs.Products;

namespace FreelancerAssignment.CQRS.Products.Queries.GetAll;

public sealed record GetAllQuery : IQuery<IEnumerable<ProductResponse>>;

public sealed class GetAllQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetAllQueryHandler> logger,
    IUrlGenratorService urlGenratorService) : IQueryHandler<GetAllQuery, IEnumerable< ProductResponse>>
{
    public async Task<Result<IEnumerable<ProductResponse>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (await unitOfWork.Products.GetAllAsync(cancellationToken) is not { } products)
                return ProductErrors.ProductNotFound;

            foreach (var product in products) 
                product.Image = urlGenratorService.GetImageUrl(product.Id) ?? string.Empty;

            var response = products.Adapt<IEnumerable<ProductResponse>>();

            return Result.Success(response);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while retrive the product");
            return Error.FromException("error while retrive the product", ex.Message);

        }
    }
}
