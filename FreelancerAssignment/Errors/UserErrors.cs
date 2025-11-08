using FreelancerAssignment.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FreelancerAssignment.Errors;

public class UserErrors
{
    public static Error UsernameNotUnique =>
        Error.BadRequest("UsernameNotUnique", $"The username is already in use. Please choose a unique username.");
    public static Error EmailNotUnique =>
        Error.BadRequest("EmailNotUnique", $"The email is already in use. Please choose a unique email.");
    public static Error UserNotFound =>
        Error.NotFound("UserNotFound", "The user was not found.");
    public static Error InvalidCredentials =>
        Error.BadRequest("InvalidCredentials", "The provided credentials are invalid.");

    public static readonly Error InvalidToken
        = Error.BadRequest("Token.InvalidToken", "This Token Is Expires");

    public static readonly Error InvalidUserId
        = Error.BadRequest("Token.InvalidUserId", "there is no user with this id");

    public static readonly Error NoRefreshToken
        = Error.BadRequest("Token.NoRefreshToken", "there is no refresh tokens");
}