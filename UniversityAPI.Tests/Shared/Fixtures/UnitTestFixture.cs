using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;
using UniversityAPI.Tests.Shared.Helpers;
using UniversityAPI.Utility;
using UniversityAPI.Utility.Helpers;
using UniversityAPI.Utility.Interfaces;

namespace UniversityAPI.Tests.Shared.Fixtures
{
    public class UnitTestFixture : IDisposable
    {
        public ApplicationDbContext Context { get; }
        public UserManager<UserDM> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public IConfigHelper ConfigHelper { get; }

        public UnitTestFixture()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                                   .AddJsonFile("appsettings.Development.json", optional: true)
                                                   .AddUserSecrets<Program>()
                                                   .Build();
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Debug);
            });

            var logger = loggerFactory.CreateLogger<IConfiguration>();

            this.ConfigHelper = new ConfigHelper(config, logger);
            this.ConfigHelper.InjectStaticConfig();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            this.Context = new ApplicationDbContext(options);

            var identityOptions = Microsoft.Extensions.Options.Options.Create(new IdentityOptions()
            {
                Password = new PasswordOptions()
                {
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireUppercase = true,
                    RequireNonAlphanumeric = true,
                    RequiredLength = 12,
                    RequiredUniqueChars = 5
                }
            });
            var userStore = new UserStore<UserDM>(this.Context);
            this.UserManager = new UserManager<UserDM>(userStore, identityOptions, new PasswordHasher<UserDM>(), null, null, null, null, null, null);

            var roleStore = new RoleStore<IdentityRole>(this.Context);
            this.RoleManager = new RoleManager<IdentityRole>(roleStore, null, null, null, null);

            TestDataSeeder.SeedDataAsync(this.Context, this.RoleManager, this.UserManager).Wait();
        }

        public void Dispose() => this.Context.Dispose();
    }
}