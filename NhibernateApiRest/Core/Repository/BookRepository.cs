using Entities;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using NhibernateApiRest.Core.IRepositories;
using NhibernateInstanceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NhibernateApiRest.Core.Repository
{
	public class BookRepository : GenericRepository<Book>, IBookRepository
	{
		public BookRepository(INhibernateHelper context, ILogger logger) : base(context, logger) { }

		public override async Task<Book> GetById(int id)
		{
			try
			{
				using var session = context.OpenSession();
				return await session.Query<Book>()
					.Fetch(t => t.CoverColor)
					.Fetch(t => t.Authors)
					.Where(t => t.Id == id)
					.WithOptions(t => t.SetCacheable(true).SetCacheRegion("redis"))
					.SingleAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{Repo} All function error", typeof(BookRepository));
				return null;
			}
		}

		public new async Task<IEnumerable<BookSimple>> All()
		{
			try
			{
				using var session = context.OpenSession();
				return await session.Query<BookSimple>()
					.OrderBy(t => t.Id)
					.WithOptions(t => t.SetCacheable(true).SetCacheRegion("redis"))
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{Repo} All function error", typeof(BookRepository));
				return new List<Book>();
			}
		}
	}
}
