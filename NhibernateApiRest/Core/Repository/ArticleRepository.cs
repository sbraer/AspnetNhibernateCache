using Entities;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using NhibernateApiRest.Core.IRepositories;
using NhibernateInstanceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NhibernateApiRest.Core.Repository
{
	public class ArticleRepository : GenericRepository<Article>, IArticleRepository
	{
		public ArticleRepository(INhibernateHelper context, ILogger logger) : base(context, logger) { }

		public override async Task<IEnumerable<Article>> All()
		{
			try
			{
				using var session = context.OpenSession();
				return await session.Query<Article>()
					.OrderBy(t => t.Id)
					.WithOptions(t => t.SetCacheable(true).SetCacheRegion("redis"))
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{Repo} All function error", typeof(ArticleRepository));
				return new List<Article>();
			}
		}

		public override async Task<Article> GetById(int id)
		{
			try
			{
				using var session = context.OpenSession();
				return await session.Query<Article>()
					.Where(t => t.Id == id)
					.WithOptions(t => t.SetCacheable(true).SetCacheRegion("redis"))
					.SingleAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{Repo} All function error", typeof(ArticleRepository));
				return null;
			}
		}

		public override async Task<IEnumerable<Article>> Find(Expression<Func<Article, bool>> predicate)
		{
			using var session = context.OpenSession();
			return await session.Query<Article>()
				.Where(predicate)
				.WithOptions(t => t.SetCacheable(true).SetCacheRegion("redis"))
				.ToListAsync();
		}
	}
}
