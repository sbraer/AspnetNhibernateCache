using NHibernate;
using NHibernate.Caches.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace Nhibernate.Caches.Redis.Core
{
	public class RedisCacheSectionHandler : ICacheConfigurationSectionHandler, IConfigurationSectionHandler
	{
		private static readonly INHibernateLogger Log = NHibernateLogger.For(typeof(RedisCacheSectionHandler));
		public string ConfigurationSectionName => "rediscache";

		public object Create(object parent, object configContext, XmlNode section)
		{
			List<RegionConfig> list = new List<RegionConfig>();
			bool cacheTypeRedisUsed = false;
			foreach (XmlNode item in section.SelectNodes("cache"))
			{
				string text = item.Attributes["region"]?.Value;
				string expiration = item.Attributes["expiration"]?.Value;
				CacheType cacheType = CacheType.Memory;
				string cacheTypeString = item.Attributes["cachetype"]?.Value;
				
				if (!string.IsNullOrEmpty(cacheTypeString))
				{
					if (Enum.TryParse(item.Attributes["cachetype"].Value, out CacheType cacheTypeBuffer))
					{
						cacheType = cacheTypeBuffer;
					}
					else
					{
						throw new ArgumentException("cachetype value error. Accepted values: Memory, Redis");
					}
				}

				if (text != null)
				{
					list.Add(new RegionConfig(text, expiration, cacheType));
					if (cacheType == CacheType.Redis)
					{
						cacheTypeRedisUsed = true;
					}

					continue;
				}
				
				Log.Warn("Found a cache region node lacking a region name: ignored. Node: {0}", item.OuterXml);
			}

			var redisSection = section.SelectSingleNode("redis");
			string connectionString = null;
			if (redisSection != null)
			{
				connectionString = cacheTypeRedisUsed ? redisSection.Attributes["connectionString"]?.Value : null;
				if (string.IsNullOrEmpty(connectionString) && cacheTypeRedisUsed)
				{
					throw new ArgumentException("Missing 'connectionString' value in redis section");
				}
			}

			return new CacheConfig(section.Attributes?["expiration-scan-frequency"]?.Value, connectionString, list.ToArray());
		}
	}
}
