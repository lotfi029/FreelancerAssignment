using FreelancerAssignment.CQRS.Users.Queries.Profile;
using FreelancerAssignment.Extensions;

namespace FreelancerAssignment.Endpoints;

public class UserEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/users")
            .WithTags(Tags.Users)
            .RequireAuthorization();

        group.MapGet("/profile", GetProfile);
        
    }
    private async Task<IResult> GetProfile(
        [FromServices] ISender sender,
        HttpContext httpContext,
        CancellationToken ct
        )
    {
        var userId = Guid.Parse(httpContext.GetUserId());
        var query = new GetProfileQuery(userId);
        var result = await sender.Send(query, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblem();
    }
}
