[![Buy Me A Coffee](https://miro.medium.com/v2/resize:fit:1090/0*lHgOW3tB_MfDAlBf.png)](https://buymeacoffee.com/osary00)

------------------------------------------------------------------------

# EFCore.Caching.Queries.Redis

A smart caching solution for Entity Framework Core queries, leveraging
Redis as a distributed cache. This library simplifies caching query
results, reducing database load and improving performance.

## Features

-   Caches query results using Redis.
-   Supports caching for `ToListAsync`, `FirstOrDefaultAsync`,
    `AnyAsync`, and `CountAsync`.
-   Auto-invalidation of cache on data changes.
-   Tracks and caches Include/ThenInclude queries.
-   Easily extendable and configurable.

## Installation

You can install the package via NuGet Package Manager:

``` 
Install-Package EFCore.Caching.Queries.Redis
```

Or using the .NET CLI:

``` 
 add package EFCore.Caching.Queries.Redis
```

## Usage

### 1. Configure Redis Cache in `Startup.cs`

Ensure you have Redis running and add the following services in your
`Startup.cs` or `Program.cs`:

``` 
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Replace with your Redis connection string.
});
```

### 2. Set Up `AddEFCoreCachingQueriesRedis`

``` 
services.AddEFCoreCachingQueriesRedis(); // you can set perfix for keys defualt is "EFCoreCaching and you can set Cache Duration defualt is 10 Minuts".
```

### 3. Use `SmartCachingDbContext`

Create a custom `DbContext` inheriting from `SmartCachingDbContext`:

``` 
class ApplicationDbContext : SmartCachingDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICacheService cacheService)
        : base(options, cacheService)
    {
    }

    public DbSet<Product> Products { get; set; }
}
```

### 4. Query with Caching

#### Cache a List of Results

``` 
var products = await context.FromCacheListAsync(context.Products.Where(p => p.Price > 100));
```

#### Cache FirstOrDefault Result

``` 
var product = await context.FromCacheFirstOrDefaultAsync(context.Products.Where(p => p.Id == 1));
```

#### Cache a Boolean Result (`Any`)

``` 
var exists = await context.FromCacheAnyAsync(context.Products.Where(p => p.Stock > 0));
```

#### Cache a Count Result

``` 
var count = await context.FromCacheCountAsync(context.Products.Where(p => p.Price > 50));
```

## Customization

- **Custom Cache Duration**: Override default cache durations by passing a `TimeSpan` parameter when querying.

### 5. Cache Invalidation

The cache automatically invalidates entries when `SaveChangesAsync` is
called:

``` 
await context.SaveChangesAsync();
```

## Cache Key Generation

The package automatically generates unique cache keys based on query
strings and includes paths.


------------------------------------------------------------------------
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

