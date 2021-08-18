using NHibernate;
using NHibernate.Cache;
using NHibernate.Caches.Common;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nhibernate.Caches.Redis.Core
{
	public class RedisCacheProvider : ICacheProvider
	{
		private static readonly INHibernateLogger Log;
		private static readonly Dictionary<string, IDictionary<string, string>> ConfiguredCachesProperties;
		public static TimeSpan? ExpirationScanFrequency { get; set; }

		public static string RedisConnectionString { get; set; }
		private static ConnectionMultiplexer RedisConnection { get; set; } = null;

		public static void SetRegionConfiguration(RegionConfig configuration)
		{
			ConfiguredCachesProperties[configuration.Region] = configuration.Properties;
		}

		static RedisCacheProvider()
		{
			Log = NHibernateLogger.For(typeof(RedisCacheProvider));
			ConfiguredCachesProperties = new Dictionary<string, IDictionary<string, string>>();
			CacheConfig configuration = ConfigurationProviderBase<CacheConfig, RedisCacheSectionHandler>.Current.GetConfiguration();
			if (configuration == null)
			{
				return;
			}

			if (configuration.ExpirationScanFrequency != null)
			{
				if (int.TryParse(configuration.ExpirationScanFrequency, out var result))
				{
					ExpirationScanFrequency = TimeSpan.FromMinutes(result);
				}
				else if (TimeSpan.TryParse(configuration.ExpirationScanFrequency, out TimeSpan result2))
				{
					ExpirationScanFrequency = result2;
				}

				if (!ExpirationScanFrequency.HasValue)
				{
					Log.Warn("Invalid value '{0}' for expiration-scan-frequency setting: it is neither an int nor a TimeSpan. Ignoring.", configuration.ExpirationScanFrequency);
				}
			}

			RegionConfig[] regions = configuration.Regions;
			foreach (RegionConfig regionConfig in regions)
			{
				ConfiguredCachesProperties.Add(regionConfig.Region, regionConfig.Properties);
			}

			RedisConnectionString = configuration.RedisConnectionString;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public ICache BuildCache(string regionName, IDictionary<string, string> properties)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if (regionName == null)
			{
				regionName = string.Empty;
			}
			if (ConfiguredCachesProperties.TryGetValue(regionName, out var value) && value.Count > 0)
			{
				if (properties != null)
				{
					properties = new Dictionary<string, string>(properties);
					foreach (KeyValuePair<string, string> item in value)
					{
						properties[item.Key] = item.Value;
					}
				}
				else
				{
					properties = value;
				}
			}
			if (properties == null)
			{
				properties = new Dictionary<string, string>(1);
			}
			if (Log.IsDebugEnabled())
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> property in properties)
				{
					stringBuilder.Append("name=");
					stringBuilder.Append(property.Key);
					stringBuilder.Append("&value=");
					stringBuilder.Append(property.Value);
					stringBuilder.Append(";");
				}

				Log.Debug("building cache with region: {0}, properties: {1}", regionName, stringBuilder.ToString());
			}

			return new RedisCache(regionName, properties, RedisConnection);
		}

		public long NextTimestamp()
		{
			return Timestamper.Next();
		}

		public void Start(IDictionary<string, string> properties)
		{
			Log.Info("Start Nhibernate.Caches.Redis.Core");
			if (!string.IsNullOrEmpty(RedisConnectionString))
			{
				try
				{
					RedisConnection = ConnectionMultiplexer.Connect(RedisConnectionString);
				}
				catch (Exception ex)
				{
					throw new Exception($"Error in Redis connection: {ex.Message}");
				}
			}
		}

		public void Stop()
		{
			Log.Info("Stop Nhibernate.Caches.Redis.Core");
			RedisConnection?.Dispose();
		}
	}
}
