namespace FreelancerAssignment.CQRS.Products.Queries.Images;

public sealed record GetImageQuery(Guid Id) : IQuery<(FileStream? stream, string? contentType, string? fileName)>;

public sealed class GetImageQueryHandler(
    IFileService fileService,
    IUnitOfWork unitOfWork) : IQueryHandler<GetImageQuery, (FileStream? stream, string? contentType, string? fileName)>
{
    public async Task<Result<(FileStream? stream, string? contentType, string? fileName)>> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Products.FindAsync(cancellationToken, request.Id) is not { } product)
            return ProductErrors.ProductNotFound;


        var result = await fileService.StreamAsync(product.Image, cancellationToken);

        if (result.fileName == null || result.contentType == null || result.stream == null)
            return Error.BadRequest("Image.NotFound", "image not found");

        return result;
    }

}