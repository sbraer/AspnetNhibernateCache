namespace Nhibernate.Caches.Redis.Core
{
	public class CacheConfig
	{
		public string ExpirationScanFrequency { get; }
		public RegionConfig[] Regions { get; }
		public string RedisConnectionString {get; }

		public CacheConfig(string expirationScanFrequency, string redisConnectionString, RegionConfig[] regions)
		{
			ExpirationScanFrequency = expirationScanFrequency;
			RedisConnectionString = redisConnectionString;
			Regions = regions;
		}
	}
}
