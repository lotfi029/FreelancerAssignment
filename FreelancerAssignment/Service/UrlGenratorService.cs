namespace FreelancerAssignment.Service;

public class UrlGenratorService(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator) : IUrlGenratorService
{
    public string? GetImageUrl(Guid productId)
    {
        var httpContext = httpContextAccessor.HttpContext!;

        return linkGenerator.GetUriByName(
            httpContext,
            endpointName: "GetImage",
            values: new { id = productId},
            scheme: httpContext.Request.Scheme,
            host: httpContext.Request.Host
            );
    }
}