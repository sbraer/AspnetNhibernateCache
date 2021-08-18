using Entities;
using FluentNHibernate.Mapping;

namespace NhibernateInstanceHelper.Mapping
{
	public class ColorMap : ClassMap<Color>
	{
		public ColorMap()
		{
			Id(x => x.Id);
			Map(x => x.Name).Not.Nullable().Index("ColorName").Length(20);
			Cache.ReadOnly().Region("redis");
			Table("Colors");
		}
	}
}
