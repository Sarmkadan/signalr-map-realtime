// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Data.Repositories;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Base implementation of the generic repository pattern.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class BaseRepository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the BaseRepository.
    /// </summary>
    public BaseRepository(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities of type T.
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds entities matching the specified predicate.
    /// </summary>
    public async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await Task.FromResult(_dbSet.Where(predicate).ToList());
    }

    /// <summary>
    /// Finds a single entity matching the specified predicate.
    /// </summary>
    public async Task<T?> FindSingleAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await Task.FromResult(_dbSet.FirstOrDefault(predicate));
    }

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Adds multiple entities to the repository.
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes an entity from the repository.
    /// </summary>
    public async Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes multiple entities from the repository.
    /// </summary>
    public async Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.RemoveRange(entities);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes an entity by its primary key.
    /// </summary>
    public async Task RemoveByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if an entity with the specified key exists.
    /// </summary>
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken) != null;
    }

    /// <summary>
    /// Gets the total count of entities.
    /// </summary>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }
}
