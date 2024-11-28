using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.Caching.Queries.Redis;
public interface ICacheService
{
    Task<T> GetFromCacheAsync<T>(string cacheKey);
    Task SetCacheAsync<T>(string cacheKey, T value, TimeSpan expiration);
    Task RemoveCacheAsync(string cacheKey);
}

