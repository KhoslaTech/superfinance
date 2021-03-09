using ASKSource.DependencyInjection;
using ASKSource.Middlewares;
using SuperFinance.DataModels;
using ASPSecurityKit;
using ASPSecurityKit.NetCore;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperFinance.DependencyInjection;

namespace SuperFinance
{
	public class ASPSecurityKitConfiguration
	{
		public static bool IsDevelopmentEnvironment { get; set; }

		public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			services.AddControllersWithViews(options =>
				{
					options.Filters.Add(typeof(ProtectAttribute));
				})
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.PropertyNamingPolicy = null;
				});

			services.AddDbContext<DemoDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

			services.AddHttpContextAccessor();
		}

		public static void ConfigureContainer(ContainerBuilder builder)
		{
			License.TryRegisterFromExecutionPath();

			// Register all ASK components and auth definitions
			new ASPSecurityKitRegistry()
				.Register(new ASKContainerBuilder(builder), authRequestDefinitionType: typeof(AuthDefinitions.BranchAuthDefinition));

			builder.RegisterModule<AppRegistry>();
			builder.RegisterModule<SFAppRegistry>();
		}

		public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			IsDevelopmentEnvironment = env.IsDevelopment();

			if (IsDevelopmentEnvironment)
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseAppExceptionHandler();
			}

			app.UseAuthSessionCaching();
		}
	}
}