using System.Collections.Generic;

namespace Entities
{
	public class BookSimple
	{
		public virtual int Id { get; set; }
		public virtual string Title { get; set; }
	}

	public class Book : BookSimple
	{
		public Book()
		{
			Authors = new List<Author>();
		}

		public virtual int NumOfPages { get; set; }
		public virtual decimal Price { get; set; }
		public virtual IList<Author> Authors { get; set; }
		public virtual Color CoverColor { get; set; }
	}
}
