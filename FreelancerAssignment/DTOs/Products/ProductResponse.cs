namespace FreelancerAssignment.DTOs.Products;

public record ProductResponse(
    Guid Id,
    string Name,
    string Category,
    string ProductCode,
    string Image,
    decimal Price,
    int MinimumQuantity,
    decimal? Discount = null
    );