using System;
using System.Linq;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bcr = BCrypt.Net;

using WebApplication.Entities;
using WebApplication.Helpers;

namespace WebApplication
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                try
                {
                    context.Database.EnsureCreated();

                    if (!context.Users.Any())
                    {
                        logger.LogInformation("No users are registered. Creating the administrator user.");
                        
                        context.Users.Add(new User
                        {
                            Username = "admin",
                            Email = "admin@minitwit.com",
                            Password = Bcr.BCrypt.HashPassword("admin")
                        });

                        context.SaveChanges();
                    }

                    logger.LogInformation("Ensured the database was created");
                }
                catch (Exception exception)
                {
                    logger.LogCritical(exception, "Failed to ensure the database was created");

                    return 1;
                }
            }
                
            host.Run();

            return 0;
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => 
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSentry();
                });
        }
    }
}
