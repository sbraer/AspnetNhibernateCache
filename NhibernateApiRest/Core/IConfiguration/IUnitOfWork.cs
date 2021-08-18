using NhibernateApiRest.Core.IRepositories;
using System.Threading.Tasks;

namespace NhibernateApiRest.Core.IConfiguration
{
	public interface IUnitOfWork
    {
        IArticleRepository Articles { get; }
        IBookRepository Books { get; }

        Task CompleteAsync();
    }

}
