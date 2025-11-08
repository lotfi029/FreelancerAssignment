using FreelancerAssignment.CQRS.Products.Commands.Add;
using FreelancerAssignment.CQRS.Products.Commands.AddImages;
using FreelancerAssignment.CQRS.Products.Commands.Delete;
using FreelancerAssignment.CQRS.Products.Commands.Update;
using FreelancerAssignment.CQRS.Products.Queries.Categories.GetCategories;
using FreelancerAssignment.CQRS.Products.Queries.Categories.GetPrducts;
using FreelancerAssignment.CQRS.Products.Queries.GetAll;
using FreelancerAssignment.CQRS.Products.Queries.GetById;
using FreelancerAssignment.CQRS.Products.Queries.Images;
using FreelancerAssignment.DTOs.Images;
using FreelancerAssignment.DTOs.Products;
using FreelancerAssignment.Extensions;

namespace FreelancerAssignment.Endpoints;

public class ProductEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/products")
            .WithTags(Tags.Products)
            .RequireAuthorization();

        group.MapPost("/", Add)
            .DisableAntiforgery();
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);
        group.MapGet("/{id:guid}", Get);
        group.MapGet("/", GetAll);
        group.MapGet("/categories", GetAllCategories);
        group.MapGet("/by-category/{categoryName:alpha}", GetProductByCategory);

        group.MapGet("/{id:guid}/product-image", GetProductImage);
        group.MapPost("/{id:guid}/product-image", ChangeProductImage)
            .WithName("GetImage")
            .DisableAntiforgery();

    }

    private async Task<IResult> Add(
        [FromForm] ProductRequest request,
        [FromServices] IValidator<ProductRequest> validator,
        [FromServices] ISender mediator,
        CancellationToken ct
        )
    {
        if (await validator.ValidateAsync(request, ct) is { IsValid: false } validationResult)
            return Results.ValidationProblem(validationResult.ToDictionary());


        var command = new AddProductCommand(Guid.NewGuid(), request);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess 
            ? Results.Created($"/api/products/{result.Value}", null!) 
            : result.ToProblem();

    }
    private async Task<IResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        [FromServices] IValidator<UpdateProductRequest> validator,
        [FromServices] ISender mediator,
        HttpContext httpContext,
        CancellationToken ct
        )
    {
        if (await validator.ValidateAsync(request, ct) is { IsValid: false} validationResult)
            return Results.ValidationProblem(validationResult.ToDictionary());
        var userId = httpContext.GetUserId();
        var command = new UpdateProductCommand(Guid.Parse(userId), id, request);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess 
            ? Results.NoContent() 
            : result.ToProblem();   
    }
    private async Task<IResult> Delete(
        [FromRoute] Guid id,
        [FromServices] ISender mediator,
        HttpContext httpContext,
        CancellationToken ct
        )
    {
        var userId = httpContext.GetUserId();   
        var command = new DeleteProductCommand(Guid.Parse(userId), id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess 
            ? Results.NoContent() 
            : result.ToProblem();   
    }
    private async Task<IResult> Get(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken ct
        )
    {
        var command = new GetByIdQuery(id);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblem();
    }
    private async Task<IResult> GetAll(
        [FromServices] ISender sender,
        CancellationToken ct
        )
    {
        var command = new GetAllQuery();
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblem();
    }

    private async Task<IResult> ChangeProductImage(
        [FromRoute] Guid id,
        [FromForm] ImageRequest request,
        [FromServices] IValidator<ImageRequest> validator,
        [FromServices] ISender sender,
        HttpContext httpContext,
        CancellationToken ct
        )
    {
        if (await validator.ValidateAsync(request, ct) is { IsValid: false } validationResult)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var userId = Guid.Parse(httpContext.GetUserId());

        var command = new AddImageCommand(userId, id, request.Image);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblem();
    }

    private async Task<IResult> GetProductImage(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken ct
        )
    {
        var query = new GetImageQuery(id);
        var result = await sender.Send(query, ct);

        return result.IsSuccess
            ? TypedResults.File(result.Value.stream!, result.Value.contentType!, result.Value.fileName)
            : result.ToProblem();
    }

    private async Task<IResult> GetAllCategories(
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var query = new GetCategoriesQuery();
        var result = await sender.Send(query, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblem();

    }

    private async Task<IResult> GetProductByCategory(
        [FromRoute] string categoryName,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var query = new GetProductByCategoryQuery(categoryName);
        var result = await sender.Send(query, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblem();

    }
}
