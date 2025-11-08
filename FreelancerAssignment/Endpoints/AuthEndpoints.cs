using FreelancerAssignment.CQRS.Users.Commands.Login;
using FreelancerAssignment.CQRS.Users.Commands.RefreshTokens;
using FreelancerAssignment.CQRS.Users.Commands.Register;
using FreelancerAssignment.DTOs.Users;
using FreelancerAssignment.Extensions;

namespace FreelancerAssignment.Endpoints;
public class AuthEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auths")
            .WithTags(Tags.Auths);

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/refresh", RefreshToken);
        group.MapPost("/revoke-refresh-token", RevokeRefreshToken);
        group.MapGet("/check-auth-status", CheckAuthStatus)
            .RequireAuthorization();
    }

    private async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IValidator<RegisterRequest> validator,
        [FromServices] ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var command = new RegisterCommand(request.Username, request.Password, request.Email);
        var result = await sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToProblem();

        SetAuthCookies(httpContext, result.Value!.Token, result.Value.RefreshToken, result.Value.RefreshTokenExpiration);
        return Results.NoContent();
    }

    private async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        [FromServices] ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var command = new LoginCommand(request.UsernameOrEmail, request.Password);
        var result = await sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToProblem();

        SetAuthCookies(httpContext, result.Value!.Token, result.Value.RefreshToken, result.Value.RefreshTokenExpiration);
        return Results.NoContent();
    }

    private async Task<IResult> RefreshToken(
        [FromServices] ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var token = httpContext.Request.Cookies[CookieContracts.AccessToken];
        var refreshToken = httpContext.Request.Cookies[CookieContracts.RefreshToken];

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refreshToken))
            return Results.Unauthorized();

        var command = new RefreshTokenCommand(token, refreshToken);
        var result = await sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToProblem();

        SetAuthCookies(httpContext, result.Value!.Token, result.Value.RefreshToken, result.Value.RefreshTokenExpiration);
        return Results.NoContent();
    }

    private async Task<IResult> RevokeRefreshToken(
        [FromServices] ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var token = httpContext.Request.Cookies[CookieContracts.AccessToken];
        var refreshToken = httpContext.Request.Cookies[CookieContracts.RefreshToken];

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refreshToken))
            return Results.Unauthorized();

        var command = new RevokeRefreshTokenCommand(token, refreshToken);
        var result = await sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToProblem();

        DeleteAuthCookies(httpContext);
        return Results.NoContent();
    }

    private IResult CheckAuthStatus() => Results.NoContent();


    private static void SetAuthCookies(HttpContext httpContext, string accessToken, string refreshToken, DateTimeOffset expires)
    {
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Expires = expires
        };

        httpContext.Response.Cookies.Append(CookieContracts.AccessToken, accessToken, cookieOptions);
        httpContext.Response.Cookies.Append(CookieContracts.RefreshToken, refreshToken, cookieOptions);
    }

    
    private static void DeleteAuthCookies(HttpContext httpContext)
    {
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        };

        httpContext.Response.Cookies.Delete(CookieContracts.AccessToken, cookieOptions);
        httpContext.Response.Cookies.Delete(CookieContracts.RefreshToken, cookieOptions);
    }

}