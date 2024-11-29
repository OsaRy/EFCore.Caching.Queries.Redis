using EFCore.Caching.Queries.Redis;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public abstract class SmartCachingDbContext: DbContext 
{
    private readonly ICacheService _cacheService;

    public SmartCachingDbContext(DbContextOptions options, ICacheService cacheService)
        : base(options)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<List<T>> FromCacheListAsync<T>(IQueryable<T> query, TimeSpan? cacheDuration = null)
    {
        var cacheKey = GenerateCacheKey(query, out var includePaths);
        Console.WriteLine($"Generating cache key: {cacheKey}");

        RegisterQueryCacheKey<T>(cacheKey, includePaths);

        var cachedData = await _cacheService.GetFromCacheAsync<List<T>>(cacheKey);
        if (cachedData != null)
        {
            Console.WriteLine($"Cache hit for key: {cacheKey}");
            return cachedData;
        }

        var result = await query.ToListAsync();
        if (result != null)
        {
            await _cacheService.SetCacheAsync(cacheKey, result, cacheDuration ?? CacheKeys.CacheDuration);
            Console.WriteLine($"Cache stored for key: {cacheKey}");
        }

        return result;
    }

    public async Task<T> FromCacheFirstOrDefaultAsync<T>(IQueryable<T> query, TimeSpan? cacheDuration = null)
    {
        var cacheKey = GenerateCacheKey(query, out var includePaths);
        Console.WriteLine($"Generating cache key: {cacheKey}");

        RegisterQueryCacheKey<T>(cacheKey, includePaths);

        var cachedData = await _cacheService.GetFromCacheAsync<T>(cacheKey);
        if (cachedData != null)
        {
            Console.WriteLine($"Cache hit for key: {cacheKey}");
            return cachedData;
        }

        var result = await query.FirstOrDefaultAsync();
        if (result != null)
        {
            await _cacheService.SetCacheAsync(cacheKey, result, cacheDuration ?? CacheKeys.CacheDuration);
            Console.WriteLine($"Cache stored for key: {cacheKey}");
        }

        return result;
    }
    public async Task<bool> FromCacheAnyAsync<T>(IQueryable<T> query, TimeSpan? cacheDuration = null)
    {
        var cacheKey = GenerateCacheKey(query, out var includePaths);
        Console.WriteLine($"Generating cache key: {cacheKey}");

        RegisterQueryCacheKey<T>(cacheKey, includePaths);

        var cachedData = await _cacheService.GetFromCacheAsync<bool>(cacheKey);
        if (cachedData)
        {
            Console.WriteLine($"Cache hit for key: {cacheKey}");
            return cachedData;
        }

        var result = await query.AnyAsync();
        await _cacheService.SetCacheAsync(cacheKey, result, cacheDuration ?? CacheKeys.CacheDuration);
        Console.WriteLine($"Cache stored for key: {cacheKey}");

        return result;
    }
    public async Task<int> FromCacheCountAsync<T>(IQueryable<T> query, TimeSpan? cacheDuration = null)
    {
        var cacheKey = GenerateCacheKey(query, out var includePaths);
        Console.WriteLine($"Generating cache key: {cacheKey}");

        RegisterQueryCacheKey<T>(cacheKey, includePaths);

        var cachedData = await _cacheService.GetFromCacheAsync<int>(cacheKey);
        if (cachedData != default)
        {
            Console.WriteLine($"Cache hit for key: {cacheKey}");
            return cachedData;
        }

        var result = await query.CountAsync();
        await _cacheService.SetCacheAsync(cacheKey, result, cacheDuration ?? CacheKeys.CacheDuration);
        Console.WriteLine($"Cache stored for key: {cacheKey}");

        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changedEntityTypes = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Select(e => e.Entity.GetType())
            .Distinct();

        var tasks = changedEntityTypes.Select(InvalidateCacheForEntityType);
        await Task.WhenAll(tasks);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task InvalidateCacheForEntityType(Type entityType)
    {
        if (CacheKeys.entityCacheKeyMap.TryGetValue(entityType, out var cacheKeys))
        {
            var tasks = cacheKeys.Select(async cacheKey =>
            {
                await _cacheService.RemoveCacheAsync(cacheKey);
                Console.WriteLine($"Invalidating cache for key: {cacheKey}");
            });

            await Task.WhenAll(tasks);

            CacheKeys.entityCacheKeyMap[entityType].Clear();
        }
    }

    private void RegisterQueryCacheKey<T>(string cacheKey, List<string> includePaths)
    {
        var mainEntityType = typeof(T);
        RegisterCacheKeyForType(mainEntityType, cacheKey);

        foreach (var includePath in includePaths)
        {
            var relatedEntityType = GetEntityTypeFromPath<T>(includePath);
            if (relatedEntityType != null)
            {
                RegisterCacheKeyForType(relatedEntityType, cacheKey);
            }
        }
    }

    private void RegisterCacheKeyForType(Type entityType, string cacheKey)
    {
        if (!CacheKeys.entityCacheKeyMap.ContainsKey(entityType))
        {
            CacheKeys.entityCacheKeyMap[entityType] = new HashSet<string>();
        }

        if (!CacheKeys.entityCacheKeyMap[entityType].Contains(cacheKey))
        {
            CacheKeys.entityCacheKeyMap[entityType].Add(cacheKey);
            Console.WriteLine($"Registered cache key for entity type {entityType.Name}: {cacheKey}");
        }
    }

    private string GenerateCacheKey<T>(IQueryable<T> query, out List<string> includePaths)
    {
        includePaths = GetIncludePaths(query);
        var queryString = query.ToQueryString();
        var combinedKey = $"{queryString} | Includes: {string.Join(", ", includePaths)}";
        return CacheKeyGenerator.GenerateKey(combinedKey);
    }

    private List<string> GetIncludePaths<T>(IQueryable<T> query)
    {
        var includePaths = new List<string>();

        void ExtractIncludes(Expression expression, string currentPath)
        {
            if (expression is MethodCallExpression methodCall)
            {
                if (methodCall.Method.Name == "Include" || methodCall.Method.Name == "ThenInclude")
                {
                    var lambda = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;
                    var memberExpression = lambda.Body as MemberExpression;

                    var includePath = string.IsNullOrEmpty(currentPath)
                        ? memberExpression.Member.Name
                        : $"{currentPath}.{memberExpression.Member.Name}";

                    includePaths.Add(includePath);

                    ExtractIncludes(methodCall.Arguments[0], string.Empty);
                }
                else
                {
                    ExtractIncludes(methodCall.Arguments[0], currentPath);
                }
            }
        }

        ExtractIncludes(query.Expression, string.Empty);
        return includePaths;
    }

    private Type GetEntityTypeFromPath<T>(string includePath)
    {
        var entityType = typeof(T);
        foreach (var nav in includePath.Split('.'))
        {
            var propInfo = entityType.GetProperty(nav);
            if (propInfo != null)
            {
                entityType = propInfo.PropertyType;
                if (entityType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(entityType.GetGenericTypeDefinition()))
                {
                    entityType = entityType.GenericTypeArguments[0];
                }
            }
        }
        return entityType == typeof(T) ? null : entityType;
    }
}
