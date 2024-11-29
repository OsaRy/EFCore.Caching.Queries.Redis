using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace EFCore.Caching.Queries.Redis
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            MaxDepth = 64
        };

        public CacheService(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<T> GetFromCacheAsync<T>(string cacheKey)
        {
            var cachedData = await _cache.GetStringAsync($"{CacheKeys.Perfix}{cacheKey}");
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<T>(cachedData, _jsonSerializerOptions);
            }

            return default;
        }

        public async Task SetCacheAsync<T>(string cacheKey, T value, TimeSpan expiration)
        {
            var serializedData = JsonSerializer.Serialize(value, _jsonSerializerOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync($"{CacheKeys.Perfix}{cacheKey}", serializedData, options);
        }

        public async Task RemoveCacheAsync(string cacheKey)
        {
            await _cache.RemoveAsync($"{CacheKeys.Perfix}{cacheKey}");
        }
    }

}
