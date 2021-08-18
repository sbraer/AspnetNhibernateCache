using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NhibernateApiRest.Core.IConfiguration;
using NhibernateApiRest.Data;
using NhibernateInstanceHelper;

namespace NhibernateApiRest
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSingleton<IUnitOfWork, UnitOfWork>();
			services.AddSingleton<INhibernateHelper>(t=> GetInstance());
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private NhibernateHelper GetInstance()
		{
			var mysql = Configuration.GetSection("MySql");
			string server = mysql.GetValue<string>("Server");
			string database = mysql.GetValue<string>("Database");
			string username = mysql.GetValue<string>("Username");
			string password = mysql.GetValue<string>("Password");
			return new NhibernateHelper(server, database, username, password);
		}
	}
}
