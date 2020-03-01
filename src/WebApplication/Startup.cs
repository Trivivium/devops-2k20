using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApplication.Entities;
using WebApplication.Helpers;
using WebApplication.Services;

namespace WebApplication
{
    public class Startup
    {

        private readonly IWebHostEnvironment _env; 

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {

            _env = env;
            Configuration = configuration;
        }

        private IConfiguration Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
             services.AddDbContext<DatabaseContext>(opts => {
                    opts.UseSqlServer("Server=db;Database=master;User=sa;Password=ULA2V9sPbG;");
                });
           

            services.AddTransient<TimelineService>();
            services.AddTransient<UserService>();
            
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
            
            app.UseStaticFiles();
            app.UseAuthentication();
            
            app.Use(async (context, next) => 
            {
                var dbContext = context.Request.HttpContext.RequestServices.GetRequiredService<DatabaseContext>();
                
                if(context.Request.Query.TryGetValue("latest", out var testString) && int.TryParse(testString, out var latest)) 
                {
                    await TestingUtils.SetLatest(dbContext, latest, context.RequestAborted);
                }
                
                await next.Invoke();
            });
            
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
