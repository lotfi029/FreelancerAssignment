using FreelancerAssignment.Entities;
using Microsoft.EntityFrameworkCore;

namespace FreelancerAssignment.Presistence.Repositories;

public class ProductRepository(
    ApplicationDbContext context,
    ILogger<Repository<Product>> repositoryLogger) : Repository<Product>(context, repositoryLogger), IProductRepository
{
    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _context.Products.Select(p => p.Category)
            .Distinct()
            .ToListAsync(cancellationToken);

        return categories;   
    }
}
