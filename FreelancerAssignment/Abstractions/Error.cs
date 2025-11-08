namespace FreelancerAssignment.Abstractions;

public record Error(string Code, string Description, int? Status)
{
    public static Error Non
        => new(string.Empty, string.Empty, null);

    public static Error BadRequest(string Code, string Description)
        => new(Code, Description, 400);

    public static Error NotFound(string Code, string Description)
        => new(Code, Description, 404);

    public static Error Conflict(string Code, string Description)
        => new(Code, Description, 409);

    public static Error Unauthorized(string Code, string Description)
        => new(Code, Description, 401);

    public static Error Forbidden(string Code, string Description)
        => new(Code, Description, 403);

    public static Error FromException(string  Code, string Description)
        => new(Code, Description, 500);

    public static Error Unexpected(string Description)
        => new("Unexpected", Description, 500);
}