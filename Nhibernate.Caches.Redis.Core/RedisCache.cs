using NHibernate.Cache;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nhibernate.Caches.Redis.Core
{
#pragma warning disable CS0618 // Type or member is obsolete
	public class RedisCache : RedisCacheBase, ICache
#pragma warning restore CS0618 // Type or member is obsolete
	{
		public new string RegionName => base.RegionName;

		public new int Timeout => base.Timeout;

		public RedisCache()
		{
		}

		public RedisCache(string region)
			: base(region)
		{
		}

		public RedisCache(string region, IDictionary<string, string> properties)
			: base(region, properties)
		{
		}

		public RedisCache(string region, IDictionary<string, string> properties, ConnectionMultiplexer redisConnection)
			: base(region, properties, redisConnection)
		{
		}

		public new Task<object> GetAsync(object key, CancellationToken cancellationToken)
		{
			return base.GetAsync(key, cancellationToken);
		}

		public new Task PutAsync(object key, object value, CancellationToken cancellationToken)
		{
			return base.PutAsync(key, value, cancellationToken);
		}

		public new Task RemoveAsync(object key, CancellationToken cancellationToken)
		{
			return base.RemoveAsync(key, cancellationToken);
		}

		public new Task ClearAsync(CancellationToken cancellationToken)
		{
			return base.ClearAsync(cancellationToken);
		}

		public new Task LockAsync(object key, CancellationToken cancellationToken)
		{
			return base.LockAsync(key, cancellationToken);
		}

		public Task UnlockAsync(object key, CancellationToken cancellationToken)
		{
			return base.UnlockAsync(key, null, cancellationToken);
		}

		public new object Get(object key)
		{
			return base.Get(key);
		}

		public new void Put(object key, object value)
		{
			base.Put(key, value);
		}

		public new void Remove(object key)
		{
			base.Remove(key);
		}

		public new void Clear()
		{
			base.Clear();
		}

		public new void Destroy()
		{
			base.Destroy();
		}

		public new void Lock(object key)
		{
			base.Lock(key);
		}

		public void Unlock(object key)
		{
			Unlock(key, null);
		}

		public new long NextTimestamp()
		{
			return base.NextTimestamp();
		}
	}
}
