using FreelancerAssignment.Entities;
using FreelancerAssignment.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FreelancerAssignment.Presistence.Repositories;

public class Repository<T> : IRepository<T>
    where T : BaseEntity
{
    protected ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger<Repository<T>> _logger;

    public Repository(ApplicationDbContext context, ILogger<Repository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> AddAsync(T entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        try
        {
            await _dbSet.AddAsync(entity, ct);
            _logger.LogInformation("Added entity of type {EntityType} with ID {EntityId}", typeof(T).Name, entity.Id);
            return entity.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
            throw new InvalidOperationException($"Error adding entity of type {typeof(T).Name}", ex);
        }
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (!entities.Any())
            return;
        try
        {
            await _dbSet.AddRangeAsync(entities, ct);
            await _context.SaveChangesAsync(true, ct);
            _logger.LogInformation("Added {Count} entities of type {EntityType}", entities.Count(), typeof(T).Name);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error adding multiple entities of type {EntityType}", typeof(T).Name);
            throw new InvalidOperationException($"Error adding multiple entities of type {typeof(T).Name}", ex);
        }
    }
    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.Id == Guid.Empty)
            throw new ArgumentException($"Invalid ID for entity of type {typeof(T).Name}", nameof(entity));
        try
        {
            var existingEntity = await _dbSet.FindAsync([entity.Id], ct);
            if (existingEntity is null)
            {
                _logger.LogWarning("Entity of type {EntityType} with ID {EntityId} not found for update", typeof(T).Name, entity.Id);
                throw new InvalidOperationException($"Entity of type {typeof(T).Name} with ID {entity.Id} not found");
            }
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            _logger.LogInformation("Updated entity of type {EntityType} with ID {EntityId}", typeof(T).Name, entity.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating entity of type {EntityType} with ID {EntityId}", typeof(T).Name, entity.Id);
            throw new InvalidOperationException($"Error updating entity of type {typeof(T).Name} with ID {entity.Id}", ex);
        }
    }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FindAsync([id], ct);
        if (entity is null)
        {
            _logger.LogWarning("Entity of type {EntityType} with ID {EntityId} not found for deletion", typeof(T).Name, id);
            throw new InvalidOperationException($"Entity of type {typeof(T).Name} with ID {id} not found");
        }
        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(true, ct);
            _logger.LogInformation("Deleted entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
            throw new InvalidOperationException($"Error deleting entity of type {typeof(T).Name} with ID {id}", ex);
        }
    }
    public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        try
        {
            if (await _dbSet.SingleOrDefaultAsync(predicate, ct) is not { } lowStock)
                return 0;
            
            _logger.LogInformation("Deleted entity of type {EntityType} with ID {EntityId}", typeof(T).Name, lowStock.Id);
            _dbSet.Remove(lowStock);
            return await _context.SaveChangesAsync(true, ct);
        }
        catch
        {
            _logger.LogError("Error deleting entity of type {EntityType} matching predicate", typeof(T).Name);
            throw new InvalidOperationException($"Error deleting entity of type {typeof(T).Name} matching predicate");
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        var exists = await _dbSet.AnyAsync(e => e.Id == id, ct);
        _logger.LogDebug("Checked existence for entity of type {EntityType} with ID {EntityId}: {Exists}", typeof(T).Name, id, exists);
        return exists;
    }
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        try
        {
            var exists = await _dbSet.AnyAsync(predicate, ct);
            _logger.LogDebug("Checked existence for entity of type {EntityType} matching predicate: {Exists}", typeof(T).Name, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for entity of type {EntityType} matching predicate", typeof(T).Name);
            throw new InvalidOperationException($"Error checking existence for entity of type {typeof(T).Name} matching predicate", ex);
        }
    }
    public async Task<T?> FindAsync(CancellationToken ct = default, params object?[] keys)
    {
        try
        {
            var result = await _dbSet.FindAsync(keys, cancellationToken: ct);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entity of type {EntityType} with parameters {Parameters}", typeof(T).Name, keys);
            throw new InvalidOperationException($"Error finding entity of type {typeof(T).Name} with parameters {keys}", ex);
        }
    }
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _dbSet.ToListAsync(ct);
            _logger.LogInformation("Retrieved {Count} entities of type {EntityType}", result.Count, typeof(T).Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entities of type {EntityType}", typeof(T).Name);
            throw new InvalidOperationException($"Error retrieving entities of type {typeof(T).Name}", ex);
        }
    }
    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        try
        {
            var result = await _dbSet
                .Where(predicate).ToListAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entities of type {EntityType} matching predicate", typeof(T).Name);
            throw new InvalidOperationException($"Error retrieving entities of type {typeof(T).Name} matching predicate", ex);
        }
    }
    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        try
        {
            var entity = await _dbSet.SingleOrDefaultAsync(predicate, ct);
            if (entity != null)
                _logger.LogInformation("Retrieved entity of type {EntityType} matching predicate", typeof(T).Name);
            else
                _logger.LogWarning("No entity of type {EntityType} found matching predicate", typeof(T).Name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity of type {EntityType} matching predicate", typeof(T).Name);
            throw new InvalidOperationException($"Error retrieving entity of type {typeof(T).Name} matching predicate", ex);
        }
    }
}
