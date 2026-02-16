using ChapeauPOS.Hubs;
using ChapeauPOS.Repositories;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

            // Add services to the container (dependency injection).
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
			builder.Services.AddSingleton<IEmployeesService, EmployeesService>();
			builder.Services.AddSingleton<ITableRepository, TableRepository>();
			builder.Services.AddSingleton<ITablesService, TablesService>();
            builder.Services.AddSingleton<IOrdersRepository, OrdersRepository>();
            builder.Services.AddSingleton<IOrdersService, OrdersService>();
            builder.Services.AddSingleton<IMenuRepository, MenuRepository>();
            builder.Services.AddSingleton<IMenuService, MenuService>();	
            builder.Services.AddSingleton<IKitchenBarRepository, KitchenBarRepository>();
            builder.Services.AddSingleton<IKitchenBarService, KitchenBarService>();
            builder.Services.AddSingleton<IFinancialRepository, FinancialRepository>();
            builder.Services.AddSingleton<IFinancialService, FinancialService>();

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
    		options.UseSqlServer(Environment.GetEnvironmentVariable("ChapeauDB_CONNECTION")));



            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); // Set the timeout to 60 minutes
                options.Cookie.HttpOnly = true; // Make the cookie HTTP-only   
                options.Cookie.IsEssential = true; // Make the session cookie essential
            });
			builder.Services.AddSignalR();

            var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseSession();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<RestaurantHub>("/restaurantHub");
            });

            app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Login}/{id?}");

			app.Run();
		}
	}
}
