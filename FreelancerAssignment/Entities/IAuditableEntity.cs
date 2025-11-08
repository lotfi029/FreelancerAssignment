namespace FreelancerAssignment.Entities;

public interface IAuditableEntity
{
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public User CreatedUser { get; set; }
}