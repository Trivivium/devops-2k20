using System;
using System.IO;
using System.Reflection;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Prometheus;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using WebApplication.Auth;
using WebApplication.Entities;
using WebApplication.Helpers;
using WebApplication.Services;

namespace WebApplication
{
    public class Startup
    {
        private IConfiguration Configuration;
        
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elastic:wpv47zN8@134.209.245.96:9200"))
                {
                    IndexFormat = "minitwitlog-{0:yyyy.MM}",
                    AutoRegisterTemplate = true,
                    /*
                     * will serialize the exception into the exception field as an
                     * object with nested InnerException properties
                     */
                    CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage:true)
                })
                .CreateLogger();
        }

      

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(opts => {
                opts.UseSqlServer("Server=db;Database=MiniTwit;User=sa;Password=ULA2V9sPbG;");
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

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy(AuthPolicies.Registered, policy => policy.RequireRole(AuthRoles.Registered));
                opts.AddPolicy(AuthPolicies.Administrator, policy => policy.RequireRole(AuthRoles.Administrator));
            });

            services.AddControllersWithViews();

            services.AddSwaggerGen(opts => 
            {
                opts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MiniTwit API",
                    Version = "v1",
                    Description = "Group B - ITU DevOps 2020 course Application",
                    Contact = new OpenApiContact
                    {
                        Name = "Group B",
                        Email = "thbi@itu.dk"
                    }
                });
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                opts.IncludeXmlComments(xmlPath);
            });

            services.AddTransient<TestingUtils>();
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
                app.UseExceptionHandler("/error");
            }

            // Count requests for each endpoint including the method
             var counter = Metrics.CreateCounter("api_path_counter", "Counts request to the API endpoints", new CounterConfiguration()
             {
                 LabelNames = new[] {"method", "endpoint"}
             });
            
             app.Use((context, next) =>
             {
                 counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
                 return next();
             });
            app.UseStatusCodePages();
            app.UseHttpMetrics();
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(setup => 
            {
                setup.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniTwit API v1");
                setup.RoutePrefix = "swagger";
            });
            
            app.UseAuthentication();
            app.UseRouting();
            
            app.Use(async (context, next) => 
            {
                var utils = context.Request.HttpContext.RequestServices.GetRequiredService<TestingUtils>();

                if(context.Request.Query.TryGetValue("latest", out var testString) && int.TryParse(testString, out var latest))
                {
                    await utils.SetLatest(latest, context.RequestAborted);
                }
                
                await next.Invoke();
            });
            
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapMetrics();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Timeline}/{action=Timeline}/{id?}");
            });
        }
    }
}
