using NHibernate.Caches.Common;

namespace Nhibernate.Caches.Redis.Core
{
	public abstract class ConfigurationProvider : ConfigurationProviderBase<CacheConfig, RedisCacheSectionHandler>
	{
	}
}
