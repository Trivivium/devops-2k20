using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using WebApplication.Entities;
using WebApplication.Helpers;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>();
            
            services.AddAuthentication(opts =>
                    {
                        opts.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        opts.RequireAuthenticatedSignIn = false;
                    })
                .AddCookie(opts =>
                {
                    opts.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    opts.SlidingExpiration = true;
                    
                    opts.LoginPath = new PathString("/login");
                    opts.LogoutPath = new PathString("/logout");
                    opts.AccessDeniedPath = new PathString("/AccessDenied");
                });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                if(context.Request.Query.TryGetValue("latest", out string latestString) && int.TryParse(latestString, out int latest)) {
                    TestingUtils.SetLatest(latest);
                }
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });
            
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Timeline}/{action=Timeline}/{id?}");
            });
        }
    }
}
