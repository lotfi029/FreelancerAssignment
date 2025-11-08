using FreelancerAssignment.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FreelancerAssignment.Presistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IHttpContextAccessor httpContextAccessor) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }  

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var updateEntries = ChangeTracker.Entries<IAuditableEntity>();
        var currentUserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;

        
        foreach (var entityTrack in updateEntries)
        {
            switch (entityTrack.State)
            {
                case EntityState.Added:
                    entityTrack.Entity.CreatedById = Guid.Parse(currentUserId);
                    break;
                default:
                    break;
            }
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
