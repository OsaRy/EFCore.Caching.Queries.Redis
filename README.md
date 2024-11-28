
# EFCore.Caching.Queries.Redis

`EFCore.Caching.Queries.Redis` is an advanced, cache-enabled extension of `DbContext` in ASP.NET Core. It integrates seamlessly with Redis to optimize data access by caching query results and intelligently invalidating caches when data changes. This approach reduces database load and improves application performance.

---

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
  - [Creating a Derived Context](#creating-a-derived-context)
  - [Configuring Dependency Injection](#configuring-dependency-injection)
  - [Available Methods](#available_methods)
  - [Cache Invalidation](#cache-invalidation)
- [Customization](#customization)
- [Best Practices](#best-practices)
- [Contributing](#contributing)
- [License](#license)

---

## Features

- **Automatic Query Caching**: Caches query results for a specified duration, reducing database access.
- **Automatic Cache Invalidation**: Invalidates cached entries when related entities are added, updated, or deleted.
- **Extensible Architecture**: Allows easy customization and integration with other caching providers.
- **Efficient Key Management**: Generates unique cache keys based on query expressions, including related entity paths.
- **Flexible Cache Duration**: Set custom cache expiration times for each query.

---

## Installation

1. **Install required packages**:

   ```bash
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
   dotnet add package StackExchange.Redis
   ```

2. **Add the Redis caching service to your ASP.NET Core project**:

   ```bash
   dotnet add package EFCore.Caching.Queries.Redis
   ```

---

## Usage

### Creating a Derived Context

Inherit from `SmartCachingDbContext` and define your `DbSet` properties:

```csharp
using EFCore.Caching.Queries.Redis;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : SmartCachingDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICacheService cacheService)
        : base(options, cacheService)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
}
```

### Configuring Dependency Injection

Register the context and cache service in `Program.cs`:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

services.AddSingleton<ICacheService, DistributedCacheService>();
```

## Available Methods

### 1. `FromCacheListAsync<T>(IQueryable<T> query, TimeSpan? cacheDuration = null)`
- **Description**: Retrieves a list of results from the cache or database. If not cached, it caches the result for future use.
- **Parameters**:
  - `query`: The EF Core query to execute.
  - `cacheDuration`: Optional cache expiration duration (default: 10 minutes).

### 2. `FromCacheFirstOrDefaultAsync<T>(IQueryable<T> query, TimeSpan? cacheDuration = null)`
- **Description**: Retrieves the first result from the cache or database. Caches the result if not found in the cache.
- **Parameters**:
  - `query`: The EF Core query to execute.
  - `cacheDuration`: Optional cache expiration duration (default: 10 minutes).

### 3. `FromCacheAnyAsync<T>(IQueryable<T> query, TimeSpan? cacheDuration = null)`
- **Description**: Checks if any records exist for the query. Caches the result for future checks.
- **Parameters**:
  - `query`: The EF Core query to execute.
  - `cacheDuration`: Optional cache expiration duration (default: 10 minutes).

### 4. `FromCacheCountAsync<T>(IQueryable<T> query, TimeSpan? cacheDuration = null)`
- **Description**: Retrieves the count of records from the cache or database. Caches the result if not found.
- **Parameters**:
  - `query`: The EF Core query to execute.
  - `cacheDuration`: Optional cache expiration duration (default: 10 minutes).

### 5. `SaveChangesAsync(CancellationToken cancellationToken = default)`
- **Description**: Overrides the default `SaveChangesAsync` method to invalidate cache keys associated with changed entities.
- **Parameters**:
  - `cancellationToken`: Optional cancellation token.

### Cache Invalidation

`SmartCachingDbContext` automatically invalidates cache entries when changes are made to entities:

```csharp
var newProduct = new Product { Name = "New Product", IsActive = true };
_context.Products.Add(newProduct);
await _context.SaveChangesAsync();  // Cache entries related to Products will be invalidated.
```

---

## Customization

- **Custom Cache Duration**: Override default cache durations by passing a `TimeSpan` parameter when querying.
- **Cache Key Generation**: Modify the `GenerateCacheKey` method to customize cache key creation.
- **Advanced Cache Invalidation**: Override `InvalidateCacheForEntityType` to implement custom cache invalidation logic.

---

## Best Practices

- **Cache Only Expensive Queries**: Avoid caching simple or frequently changing queries.
- **Set Appropriate Expiration**: Use shorter cache durations for volatile data and longer durations for static data.
- **Monitor Cache Usage**: Use Redis monitoring tools to track cache hits and misses for performance tuning.

---

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature-name`.
3. Commit your changes: `git commit -m 'Add new feature'`.
4. Push to the branch: `git push origin feature-name`.
5. Submit a pull request.

---

## License

This project is licensed under the MIT License.

---

Feel free to reach out with suggestions or issues. Happy caching! 🚀
