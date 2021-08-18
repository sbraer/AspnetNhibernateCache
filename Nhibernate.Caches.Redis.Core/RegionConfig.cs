using System.Collections.Generic;

namespace Nhibernate.Caches.Redis.Core
{
	public enum CacheType { Redis, Memory};
	public class RegionConfig
	{
		public string Region { get; }

		public IDictionary<string, string> Properties { get; }

		public RegionConfig(string region, string expiration, CacheType cacheType)
		{
			Region = region;
			Properties = new Dictionary<string, string>();
			Properties["cachetype"] = cacheType.ToString() ?? CacheType.Memory.ToString();
			if (!string.IsNullOrEmpty(expiration))
			{
				Properties["expiration"] = expiration;
			}
		}
	}
}
