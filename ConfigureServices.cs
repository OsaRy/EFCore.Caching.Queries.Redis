using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.Caching.Queries.Redis
{
    public static class ConfigureServices
    {
        public static void AddEFCoreCachingQueriesRedis(this IServiceCollection services, string? perfix = null, TimeSpan? cacheDuration = null)
        {

            // Register DistributedCacheService
            services.AddSingleton<ICacheService, CacheService>();
            if(perfix!=null)
            CacheKeys.Perfix = perfix;

            if (cacheDuration != null)
                CacheKeys.CacheDuration = cacheDuration ?? TimeSpan.FromMinutes(10);


        }
    }
}
