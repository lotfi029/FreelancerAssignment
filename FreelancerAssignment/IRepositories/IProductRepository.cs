using FreelancerAssignment.Entities;

namespace FreelancerAssignment.IRepositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}

