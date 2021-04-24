using System;
using Market.Context;
using Market.ServiceInterfaces;
using Market.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Market
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddSession(session => { session.Cookie.IsEssential = true; }).AddMemoryCache();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });
            services
                .AddHttpContextAccessor()
                .AddAuthentication(options =>
                {
                    options.RequireAuthenticatedSignIn = true;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.LoginPath = "/User/Login";
                });
            services.AddDataProtection();

            var connectionString = "Server=localhost; Database=market_db; User Id=market_user; Password=market_secret;";
            services.AddDbContext<MarketContext>(options => options.UseNpgsql(connectionString));

            services
                .AddScoped<IPermissionService, PermissionService>()
                .AddScoped<ICategoryService, CategoryService>()
                .AddScoped<IProductService, ProductService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseStaticFiles()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseSession()
                .UseEndpoints(routes =>
                { 
                    routes.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");

                    routes.MapControllers();
                    //routes.MapFallbackToController("Login", "User");
                });

        }
    }
}