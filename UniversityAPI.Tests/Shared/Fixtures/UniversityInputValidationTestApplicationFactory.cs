using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;
using UniversityAPI.Service;
using UniversityAPI.Tests.Shared.Helpers;
using UniversityAPI.Utility;

namespace UniversityAPI.Tests.Shared.Fixtures
{
    public class UniversityInputValidationTestApplicationFactory : WebApplicationFactory<Program>
    {
        public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            using var scope = this.Services.CreateScope();
            await action(scope.ServiceProvider);
        }

        public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using var scope = this.Services.CreateScope();
            return await action(scope.ServiceProvider);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                                   .AddJsonFile("appsettings.Development.json", optional: true)
                                                   .AddUserSecrets<Program>()
                                                   .AddEnvironmentVariables()
                                                   .Build();
            builder.UseConfiguration(config);

            _ = builder.ConfigureServices(services =>
            {
                try
                {
                    var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "UniversityInputValidationDomainDb")
                        .Options;

                    services.Replace(ServiceDescriptor.Singleton(typeof(DbContextOptions<ApplicationDbContext>), new DbContextOptions<ApplicationDbContext>()));
                    services.Replace(ServiceDescriptor.Singleton(typeof(ApplicationDbContext), new ApplicationDbContext(contextOptions)));

                    services.AddUtilityLayer();
                    services.AddServiceLayer();

                    services
                        .AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                    services.PostConfigure<MvcOptions>(options =>
                    {
                        options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
                    });
                    services.Configure<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    });

                    var serviceProvider = services.BuildServiceProvider();

                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDM>>();

                    context.Database.EnsureCreated();
                    TestDataSeeder.SeedDataAsync(context, roleManager, userManager).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error configuring test services: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            });
        }
    }
}
