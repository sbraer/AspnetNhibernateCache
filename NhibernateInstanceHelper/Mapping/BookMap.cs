using Entities;
using FluentNHibernate.Mapping;

namespace NhibernateInstanceHelper.Mapping
{
	public class BookSimpleMap : ClassMap<BookSimple>
	{
		public BookSimpleMap()
		{
			Id(x => x.Id);
			Map(x => x.Title).Not.Nullable().Index("BookTitle").Length(100);
			Cache.ReadOnly().Region("redis");
			Table("Books");
		}
	}
	public class BookMap : ClassMap<Book>
	{
		public BookMap()
		{
			Polymorphism.Explicit();
			Id(x => x.Id);
			Map(x => x.Title).Not.Nullable().Index("BookTitle").Length(100);
			Map(x => x.NumOfPages).Not.Nullable();
			Map(x => x.Price).Not.Nullable();
			References(x => x.CoverColor).Column("ColorId").Not.Nullable();
			HasManyToMany(x => x.Authors).Table("Books_Authors").ParentKeyColumn("BookId").ChildKeyColumn("AuthorId").Cache.ReadWrite().Region("redis");
			Cache.ReadOnly().Region("redis");
			Table("Books");
		}
	}
}
