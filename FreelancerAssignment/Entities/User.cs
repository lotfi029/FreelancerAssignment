namespace FreelancerAssignment.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime LastLoginTime { get; set; }
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Product> CreatedProducts { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}