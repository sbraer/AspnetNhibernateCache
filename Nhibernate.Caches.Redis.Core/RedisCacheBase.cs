using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using NHibernate;
using NHibernate.Cache;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Nhibernate.Caches.Redis.Core
{
	public abstract class RedisCacheBase : CacheBase
	{
		private static readonly INHibernateLogger Log;
		private static readonly TimeSpan DefaultExpiration;
		private static readonly string DefaultRegionPrefix;
		private static readonly IMemoryCache Cache;

		private string _fullRegion;
		private volatile CancellationTokenSource _clearToken = new CancellationTokenSource();
		private readonly ReaderWriterLockSlim _clearTokenLock = new ReaderWriterLockSlim();

		public override string RegionName { get; }

		public ConnectionMultiplexer RedisConnection { get; }

		public TimeSpan Expiration { get; private set; }

		public CacheType CacheTypeRegion { get; private set; }

		public override int Timeout => 245760000;

		static RedisCacheBase()
		{
			Log = NHibernateLogger.For(typeof(RedisCache));
			DefaultExpiration = TimeSpan.FromSeconds(300.0);
			DefaultRegionPrefix = string.Empty;

			MemoryCacheOptions memoryCacheOptions = new MemoryCacheOptions();
			if (RedisCacheProvider.ExpirationScanFrequency.HasValue)
			{
				memoryCacheOptions.ExpirationScanFrequency = RedisCacheProvider.ExpirationScanFrequency.Value;
			}

			Cache = new MemoryCache(memoryCacheOptions);
		}

		public RedisCacheBase()	: this("nhibernate", null, null)
		{
		}

		public RedisCacheBase(string region) : this(region, null, null)
		{
		}

		public RedisCacheBase(string region, IDictionary<string, string> properties) : this(region, properties, null)
		{
		}

		public RedisCacheBase(string region, IDictionary<string, string> properties, ConnectionMultiplexer redisConnection)
		{
			RegionName = region;
			Configure(properties);
			RedisConnection = redisConnection;
		}

		private void Configure(IDictionary<string, string> props)
		{
			string text = DefaultRegionPrefix;
			if (props == null)
			{
				Log.Warn("Configuring cache with default values");
				Expiration = DefaultExpiration;
				CacheTypeRegion = CacheType.Memory;
			}
			else
			{
				Expiration = GetExpiration(props);
				text = GetRegionPrefix(props);
				CacheTypeRegion = GetCacheType(props);
			}

			_fullRegion = text + RegionName;
		}

		private static CacheType GetCacheType(IDictionary<string, string> props)
		{
			if (props.ContainsKey("cachetype"))
			{
				if (Enum.TryParse(props["cachetype"], out CacheType cachetype))
				{
					return cachetype;
				}
			}

			return CacheType.Memory;
		}

		private static string GetRegionPrefix(IDictionary<string, string> props)
		{
			if (props.TryGetValue("regionPrefix", out var value))
			{
				Log.Debug("new regionPrefix: {0}", value);
			}
			else
			{
				value = DefaultRegionPrefix;
				Log.Debug("no regionPrefix value given, using defaults");
			}

			return value;
		}

		private static TimeSpan GetExpiration(IDictionary<string, string> props)
		{
			TimeSpan result = DefaultExpiration;
			if (!props.TryGetValue("expiration", out var value))
			{
				props.TryGetValue("cache.default_expiration", out value);
			}
			if (value != null)
			{
				if (!int.TryParse(value, out var result2))
				{
					Log.Error("error parsing expiration value '{0}'", value);
					throw new ArgumentException("could not parse expiration '" + value + "' as a number of seconds");
				}
				result = TimeSpan.FromSeconds(result2);
				Log.Debug("new expiration value: {0}", result2);
			}
			else
			{
				Log.Debug("no expiration value given, using defaults");
			}
			return result;
		}

		private object GetCacheKey(object key)
		{
			return new Tuple<string, object>(_fullRegion, key);
		}

		public override object Get(object key)
		{
			return GetAsync(key, new CancellationToken()).Result;
		}

		public override async Task<object> GetAsync(object key, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (key == null)
			{
				return null;
			}
			object cacheKey = GetCacheKey(key);
			Log.Debug("Fetching object '{0}' from the cache.", cacheKey);

			Type type = key.GetType();
			if (type == typeof(string))
			{
				return null;
			}

			byte[] bytes;
			if (CacheTypeRegion == CacheType.Redis)
			{
				IDatabase db = GetRedisDb();
				bytes = await db.StringGetAsync(cacheKey.ToString());
			}
			else
			{
				bytes = Cache.Get(cacheKey) as byte[];
			}

			if (bytes == null || bytes.Length == 0)
			{
				return null;
			}

			using var ms = new MemoryStream(bytes);
			IFormatter formatter = new BinaryFormatter();
			return formatter.Deserialize(ms);
		}

		public override void Put(object key, object value)
		{
			PutAsync(key, value, new CancellationToken()).Wait();
		}

		public override async Task PutAsync(object key, object value, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (key == null)
			{
				throw new ArgumentNullException("key", "null key not allowed");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value", "null value not allowed");
			}

			object cacheKey = GetCacheKey(key);

			Log.Debug("putting item with key: {0} in {1}", cacheKey, CacheTypeRegion);
			Type inputType = key.GetType();
			Type outputType = value.GetType();
			if (inputType == typeof(string))
			{
				return;
			}
			else if (inputType == typeof(CacheKey))
			{
				if (outputType == typeof(CacheLock)) return;
			}

			byte[] bytes;
			IFormatter formatter = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream())
			{
				formatter.Serialize(stream, value);
				bytes = stream.ToArray();
			}

			if (CacheTypeRegion == CacheType.Redis)
			{
				IDatabase db = GetRedisDb();
				await db.StringSetAsync(cacheKey.ToString(), bytes, Expiration);
			}
			else
			{
				MemoryCacheEntryOptions memoryCacheEntryOptions = new MemoryCacheEntryOptions();
				memoryCacheEntryOptions.AbsoluteExpirationRelativeToNow = Expiration;

				_clearTokenLock.EnterReadLock();
				try
				{
					memoryCacheEntryOptions.ExpirationTokens.Add(new CancellationChangeToken(_clearToken.Token));
					Cache.Set(cacheKey, bytes, memoryCacheEntryOptions);
				}
				finally
				{
					_clearTokenLock.ExitReadLock();
				}
			}
		}

		private IDatabase GetRedisDb()
		{
			try
			{
				return RedisConnection.GetDatabase();
			}
			catch (Exception)
			{
				throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Unable to connect to Redis server");
			}
		}

		public override void Remove(object key)
		{
			RemoveAsync(key, new CancellationToken()).Wait();
		}

		public override async Task RemoveAsync(object key, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			object cacheKey = GetCacheKey(key);
			Log.Debug("removing item with key: {0}", cacheKey);

			if (CacheTypeRegion == CacheType.Redis)
			{
				IDatabase db = GetRedisDb();
				await db.KeyDeleteAsync(key.ToString());
			}
			else
			{
				Cache.Remove(cacheKey);
			}
		}

		public override void Clear()
		{
			_clearTokenLock.EnterWriteLock();
			try
			{
				_clearToken.Cancel();
				_clearToken.Dispose();
				_clearToken = new CancellationTokenSource();
			}
			finally
			{
				_clearTokenLock.ExitWriteLock();
			}
		}

		public override void Destroy()
		{
			Clear();
			_clearToken.Dispose();
			_clearTokenLock.Dispose();
		}

		public override object Lock(object key)
		{
			return null;
		}

		public override void Unlock(object key, object lockValue)
		{
		}

		public override long NextTimestamp()
		{
			return Timestamper.Next();
		}
	}
}
