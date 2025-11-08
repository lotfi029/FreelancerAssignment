namespace FreelancerAssignment.DTOs.Products;

public record ProductRequest(
    string Name,
    string Category,
    decimal Price,
    int MinimumQuantity,
    IFormFile? Image,
    decimal? Discount = null
    );
