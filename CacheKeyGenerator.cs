using System.Security.Cryptography;
using System.Text;

namespace EFCore.Caching.Queries.Redis
{
	public static class CacheKeyGenerator
	{
		public static string GenerateKey(string queryString)
		{
			using var sha1 = SHA1.Create();
			var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(queryString));
			return $"Cache_{BitConverter.ToString(hashBytes).Replace("-", "").ToLower()}";
		}
	}
}
