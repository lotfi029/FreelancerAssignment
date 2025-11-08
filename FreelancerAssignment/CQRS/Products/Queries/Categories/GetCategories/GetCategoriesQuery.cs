namespace FreelancerAssignment.CQRS.Products.Queries.Categories.GetCategories;

public record GetCategoriesQuery : IQuery<IEnumerable<string>>;
public class GetCategoriesQueryHandler(
    ILogger<GetCategoriesQueryHandler> logger,
    IUnitOfWork unitOfWork) : IQueryHandler<GetCategoriesQuery, IEnumerable<string>>
{
    public async Task<Result<IEnumerable<string>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await unitOfWork.Products.GetCategoriesAsync(cancellationToken);

            if (categories is null || !categories.Any())
                return Result.Success(Enumerable.Empty<string>());

            return Result.Success(categories);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving product categories");
            return Result.Failure<IEnumerable<string>>(Error.FromException("GetCategoriesError", ex.Message));
        }
    }
}