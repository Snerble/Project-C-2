using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using QARS.Data;
using QARS.Data.Authentication;
using QARS.Data.Models;
using QARS.Data.Services;

using System.Reflection;
using System.Threading.Tasks;

namespace QARS
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddServerSideBlazor();

			#region Database Setup
			// When in Debug mode and EntityFramework isn't running, use the connection string specified in UseConnection. Otherwise use DefaultConnection
			services.AddDbContext<AppDbContext>(options => options.UseSqlite(Configuration.GetConnectionString(
#if DEBUG
				Assembly.GetEntryAssembly().GetName().Name != "ef" ?
					Configuration.GetValue<string>("UseConnection") :
#endif
					"DefaultConnection"
			)));
			#endregion

			services.AddDefaultIdentity<User>(config =>
			{
				config.Password.RequireDigit = false;
				config.Password.RequireLowercase = false;
				config.Password.RequiredLength = 0;
				config.Password.RequireUppercase = false;
				config.Password.RequireNonAlphanumeric = false;
			})
				.AddRoles<Role>()
				.AddRoleStore<QARSRoleStore>()
				.AddUserStore<QARSUserStore>();

			#region Service Registration
			services.AddScoped<CarModelServices>();

			services.ConfigureApplicationCookie(x => x.LoginPath = "/OtherLogin");
			#endregion
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RoleManager<Role> roleManager)
		{
			// Add default roles
			SeedRolesAsync(roleManager).Wait();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();
			app.UseAuthentication();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}

		/// <summary>
		/// Seeds the database with default builtin roles.
		/// </summary>
		/// <param name="roleManager">The rolemanager with which to create the roles.</param>
		private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
		{
			string[] roles = new[]
			{
				Role.Admin,
				Role.Customer,
				Role.Employee,
				Role.Franchisee
			};

			foreach (var role in roles)
				if (!await roleManager.RoleExistsAsync(role))
					await roleManager.CreateAsync(new Role { Name = role });
		}
	}
}
