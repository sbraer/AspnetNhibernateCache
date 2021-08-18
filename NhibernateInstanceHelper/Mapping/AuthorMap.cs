using Entities;
using FluentNHibernate.Mapping;

namespace NhibernateInstanceHelper.Mapping
{
	public class AuthorMap : ClassMap<Author>
	{
		public AuthorMap()
		{
			Id(x => x.Id);
			Map(x => x.Name).Not.Nullable().Index("AuthorName").Length(100);
			Map(x => x.Sex).Not.Nullable().CustomType<Sex>();
			Cache.ReadOnly().Region("redis");
			Table("Authors");
		}
	}
}
