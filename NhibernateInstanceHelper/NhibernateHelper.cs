using Entities;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NhibernateInstanceHelper.Mapping;
using System;
using System.Collections.Generic;

namespace NhibernateInstanceHelper
{
	public interface INhibernateHelper
	{
		void CloseSession();
		void Dispose();
		ISession OpenSession();
	}

	public class NhibernateHelper : IDisposable, INhibernateHelper
	{
		private ISessionFactory _sessionFactory;
		private string _server;
		private string _databaseName;
		private string _userId;
		private string _password;

		private NhibernateHelper() => throw new NotSupportedException();
		public NhibernateHelper(string server, string databaseName, string userId, string password)
		{
			_server = server ?? throw new ArgumentNullException(nameof(server));
			_databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
			_userId = userId ?? throw new ArgumentNullException(nameof(userId));
			_password = password ?? throw new ArgumentNullException(nameof(password));
		}

		private ISessionFactory SessionFactory
		{
			get
			{
				if (_sessionFactory == null)
				{
					CreateDatabase();
					ISessionFactory sessionFactory = Fluently.Configure()
						.Database(MySQLConfiguration.Standard.ConnectionString(GetConnectionStringFluent()).UseReflectionOptimizer())
						.Mappings(m => m.FluentMappings.AddFromAssemblyOf<ArticleMap>().ExportTo("./NhibernateConfigFile"))
						.ExposeConfiguration(cfg =>
						{
#if (DEBUG)
							cfg.SetProperty("show_sql", "true");
#else
							cfg.SetProperty("show_sql", "false");
#endif
							cfg.SetProperty("cache.use_second_level_cache", "true");
							cfg.SetProperty("cache.use_query_cache", "true");
							cfg.SetProperty("cache.default_expiration", "10");
							cfg.SetProperty("cache.provider_class", "Nhibernate.Caches.Redis.Core.RedisCacheProvider, Nhibernate.Caches.Redis.Core");
							new SchemaExport(cfg).Create(true, true);
						})
						.BuildSessionFactory();

					_sessionFactory = sessionFactory;

					FillData();
				}

				return _sessionFactory;
			}
		}

		private void CreateDatabase()
		{
			try
			{
				using MySqlConnection connection = new MySqlConnection(GetConnectionString());
				using MySqlCommand command = new MySqlCommand($"CREATE DATABASE {_databaseName};", connection);
				connection.Open();
				command.ExecuteNonQuery();
				connection.Close();

				Console.WriteLine("Datebase created");
			}
			catch (Exception)
			{
				Console.WriteLine("Database already exist");
			}
		}

		private void FillData()
		{
			using ISession session = _sessionFactory.OpenSession();
			using ITransaction tx = session.BeginTransaction();

			for (int i = 0; i < 100; i++)
			{
				var a = new Article
				{
					Name = $"Article {i + 1:000}",
					Date = new DateTime(2021, 1, 1).AddDays(i),
					Price = 50 + i,
					Qty = 2000 + i,
					Description = $"Long description about this article... ({i + 1})"
				};

				session.Save(a);
			}

			var authors = new List<Author>();
			for (int i = 0; i < 10; i++)
			{
				var a = new Author
				{
					Name = $"Name Author {i + 1}",
					Sex = (i % 2 == 0) ? Sex.F : Sex.M
				};

				authors.Add(a);
				session.Save(a);
			}

			var c1 = new Color { Name = "Black" };
			session.Save(c1);
			var c2 = new Color { Name = "White" };
			session.Save(c2);
			var c3 = new Color { Name = "Red" };
			session.Save(c3);
			var c4 = new Color { Name = "Green" };
			session.Save(c4);

			var colors = new Color[] { c1, c2, c3, c4 };

			for (int i = 0; i < 100; i++)
			{
				var b = new Book
				{
					Title = $"Title Book {i + 1}",
					NumOfPages = 100 + i * 10,
					Price = 10 + i,
					CoverColor = colors[i % 4],
					Authors = new List<Author>
					{
						authors[i % 10],
						authors[3],
						authors[6]
					}
				};

				session.Save(b);
			}

			tx.Commit();
		}

		private string GetConnectionString()
		{
			return $"Server={_server};User ID={_userId};Password={_password};";
		}

		private string GetConnectionStringFluent()
		{
			return $"Server={_server};Database={_databaseName};User ID={_userId};Password={_password};";
		}

		public ISession OpenSession()
		{
			return SessionFactory.OpenSession();
		}

		public void CloseSession()
		{
			SessionFactory.Close();
		}

		public void Dispose()
		{
			SessionFactory.Dispose();
		}
	}
}
