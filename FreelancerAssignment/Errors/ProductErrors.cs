using FreelancerAssignment.Abstractions;

namespace FreelancerAssignment.Errors;

public class ProductErrors
{
    public static Error ProductCodeNotUnique =>
        Error.BadRequest("ProductCodeNotUnique", $"The product code is already in use. Please choose a unique product code.");

    public static Error ProductNotFound =>
        Error.NotFound("ProductNotFound", "The product was not found.");

    public static Error UnauthorizedAccess =>
        Error.Unauthorized("UnauthorizedAccess", "You do not have permission to perform this action.");
}
