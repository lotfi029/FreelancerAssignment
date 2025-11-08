using System.Security.Claims;

namespace FreelancerAssignment.Extensions;

public static class UserIdExtensions
{
    public static string GetUserId(this HttpContext httpContext) =>
        httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value! ??
        throw new UnauthorizedAccessException("User ID not found in claims");
}
