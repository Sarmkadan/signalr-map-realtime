# BaseRepository

A generic base repository class providing common data access operations for entity types in the `signalr-map-realtime` project. It abstracts basic CRUD operations and query capabilities against a database context, designed for use with Entity Framework Core.

## API

### `BaseRepository()`
Constructs a new instance of the repository. Requires an `ApplicationDbContext` to be provided via dependency injection.

### `async Task<T?> GetByIdAsync(int id)`
Retrieves an entity by its primary key identifier.

- **Parameters**: `id` – The primary key value of the entity to retrieve.
- **Returns**: The entity if found; otherwise, `null`.
- **Exceptions**: Throws `ArgumentException` if `id` is less than or equal to zero.

### `async Task<T?> GetByIdAsync(Guid id)`
Retrieves an entity by its primary key identifier.

- **Parameters**: `id` – The primary key value of the entity to retrieve.
- **Returns**: The entity if found; otherwise, `null`.
- **Exceptions**: Throws `ArgumentException` if `id` is an empty GUID.

### `async Task<IEnumerable<T>> GetAllAsync()`
Retrieves all entities of type `T` from the database.

- **Returns**: An enumerable collection of all entities.
- **Exceptions**: May throw `DbUpdateException` if the underlying query fails.

### `async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)`
Retrieves entities matching the specified predicate.

- **Parameters**: `predicate` – A lambda expression defining the filter criteria.
- **Returns**: An enumerable collection of entities matching the predicate.
- **Exceptions**: May throw `ArgumentNullException` if `predicate` is `null`.

### `async Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate)`
Retrieves a single entity matching the specified predicate.

- **Parameters**: `predicate` – A lambda expression defining the filter criteria.
- **Returns**: The first entity matching the predicate if found; otherwise, `null`.
- **Exceptions**: May throw `ArgumentNullException` if `predicate` is `null`.

### `async Task AddAsync(T entity)`
Adds a new entity to the repository.

- **Parameters**: `entity` – The entity to add.
- **Exceptions**: Throws `ArgumentNullException` if `entity` is `null`.

### `async Task AddRangeAsync(IEnumerable<T> entities)`
Adds multiple new entities to the repository.

- **Parameters**: `entities` – A collection of entities to add.
- **Exceptions**: Throws `ArgumentNullException` if `entities` is `null`.

### `async Task UpdateAsync(T entity)`
Updates an existing entity in the repository.

- **Parameters**: `entity` – The entity to update.
- **Exceptions**: Throws `ArgumentNullException` if `entity` is `null`.

### `async Task RemoveAsync(T entity)`
Removes an entity from the repository.

- **Parameters**: `entity` – The entity to remove.
- **Exceptions**: Throws `ArgumentNullException` if `entity` is `null`.

### `async Task RemoveRangeAsync(IEnumerable<T> entities)`
Removes multiple entities from the repository.

- **Parameters**: `entities` – A collection of entities to remove.
- **Exceptions**: Throws `ArgumentNullException` if `entities` is `null`.

### `async Task RemoveByIdAsync(int id)`
Removes an entity by its primary key identifier.

- **Parameters**: `id` – The primary key value of the entity to remove.
- **Exceptions**: Throws `ArgumentException` if `id` is less than or equal to zero.

### `async Task RemoveByIdAsync(Guid id)`
Removes an entity by its primary key identifier.

- **Parameters**: `id` – The primary key value of the entity to remove.
- **Exceptions**: Throws `ArgumentException` if `id` is an empty GUID.

### `async Task<int> SaveChangesAsync()`
Persists all pending changes to the underlying database.

- **Returns**: The number of state entries written to the database.
- **Exceptions**: May throw `DbUpdateException` if the save operation fails.

### `async Task<bool> ExistsAsync(int id)`
Determines whether an entity with the specified primary key exists.

- **Parameters**: `id` – The primary key value to check.
- **Returns**: `true` if the entity exists; otherwise, `false`.
- **Exceptions**: Throws `ArgumentException` if `id` is less than or equal to zero.

### `async Task<bool> ExistsAsync(Guid id)`
Determines whether an entity with the specified primary key exists.

- **Parameters**: `id` – The primary key value to check.
- **Returns**: `true` if the entity exists; otherwise, `false`.
- **Exceptions**: Throws `ArgumentException` if `id` is an empty GUID.

### `async Task<int> CountAsync()`
Returns the total number of entities of type `T` in the repository.

- **Returns**: The count of entities.
- **Exceptions**: May throw `DbUpdateException` if the query fails.

## Usage

### Example 1: Fetching and updating an entity
