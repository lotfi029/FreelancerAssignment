namespace FreelancerAssignment.DTOs.Products;

public record UpdateProductRequest (
    string Name,
    string Category,
    decimal Price,
    int MinimumQuantity,
    decimal? Discount = null
    );
