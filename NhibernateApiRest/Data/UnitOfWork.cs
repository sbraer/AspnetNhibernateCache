using Microsoft.Extensions.Logging;
using NhibernateApiRest.Core.IConfiguration;
using NhibernateApiRest.Core.IRepositories;
using NhibernateApiRest.Core.Repository;
using NhibernateInstanceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NhibernateApiRest.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly INhibernateHelper _context;
        private readonly ILogger _logger;

        public IArticleRepository Articles { get; private set; }
        public IBookRepository Books { get; private set; }

        public UnitOfWork(INhibernateHelper context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("logs");

            Articles = new ArticleRepository(context, _logger);
            Books = new BookRepository(context, _logger);
        }

        public async Task CompleteAsync()
        {
            await Task.Yield();
            throw new NotImplementedException();
            //await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
