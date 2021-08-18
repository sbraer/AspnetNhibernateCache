using System;

namespace Entities
{
	public class Article
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual decimal Price { get; set; }
		public virtual long Qty { get; set; }
		public virtual string Description { get; set; }
	}
}
