using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.Caching.Queries.Redis
{
	public static class CacheKeys
	{
		public static  ConcurrentDictionary<Type, HashSet<string>> entityCacheKeyMap = new();

	}
}
