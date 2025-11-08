using FreelancerAssignment.DTOs.Products;

namespace FreelancerAssignment.CQRS.Products.Queries.Categories.GetPrducts;

public record GetProductByCategoryQuery(string Category) : IQuery<IEnumerable<ProductResponse>>;

public class GetProductByCategoryQueryHandler(
    ILogger<GetProductByCategoryQuery> logger,
    IUnitOfWork unitOfWork,
    IUrlGenratorService urlGenratorService) : IQueryHandler<GetProductByCategoryQuery, IEnumerable<ProductResponse>>
{
    public async Task<Result<IEnumerable<ProductResponse>>> Handle(GetProductByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var products = await unitOfWork.Products.GetAllAsync(e => e.Category == request.Category, cancellationToken);

            if (products is null || !products.Any())
                return Result.Success(Enumerable.Empty<ProductResponse>());

            foreach (var product in products)
                product.Image = urlGenratorService.GetImageUrl(product.Id) ?? string.Empty;

            var response = products.Adapt<IEnumerable<ProductResponse>>();

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving product categories");
            return Result.Failure<IEnumerable<ProductResponse>>(Error.FromException("GetCategoriesError", ex.Message));
        }
    }
}