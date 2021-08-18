using Microsoft.Extensions.Logging;
using NhibernateApiRest.Core.IRepositories;
using NhibernateInstanceHelper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NhibernateApiRest.Core.Repository
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected INhibernateHelper context;
        public readonly ILogger _logger;

        public GenericRepository(
            INhibernateHelper context,
            ILogger logger)
        {
            this.context = context;
            _logger = logger;
        }

        public virtual async Task<T> GetById(int id)
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        public virtual async Task<IEnumerable<T>> All()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        public virtual async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
        {
            await Task.Yield();
            throw new NotImplementedException();
        }
    }
}
