using Entities;
using FluentNHibernate.Mapping;

namespace NhibernateInstanceHelper.Mapping
{
	public class ArticleMap : ClassMap<Article>
	{
		public ArticleMap()
		{
			Id(x => x.Id);
			Map(x => x.Name).Not.Nullable().Index("ArticleName").Length(100);
			Map(x => x.Price).Not.Nullable();
			Map(x => x.Date).Not.Nullable();
			Map(x => x.Qty).Not.Nullable();
			Map(x => x.Description).Length(200);
			Cache.ReadOnly().Region("redis");
			Table("Articles");
		}
	}
}
