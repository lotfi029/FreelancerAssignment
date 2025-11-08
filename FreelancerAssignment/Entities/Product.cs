
namespace FreelancerAssignment.Entities;

public class Product : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MinimumQuantity { get; set; } 
    public decimal Discount { get; set; }
    
    public ICollection<User> Users { get; set; } = [];
    public DateTime? UpdatedAt { get ; set ; }
    public Guid CreatedById { get; set; }
    public User CreatedUser { get; set; } = default!;
}
