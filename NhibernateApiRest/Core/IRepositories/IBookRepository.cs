using Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhibernateApiRest.Core.IRepositories
{
	public interface IBookRepository : IGenericRepository<Book>
	{
		new Task<IEnumerable<BookSimple>> All();
	}

}
