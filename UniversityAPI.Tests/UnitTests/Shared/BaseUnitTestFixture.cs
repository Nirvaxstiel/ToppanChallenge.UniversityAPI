namespace UniversityAPI.Tests.UnitTests.Shared
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using UniversityAPI.Framework.Database;
    using UniversityAPI.Framework.Model.User;
    using UniversityAPI.Tests.Shared.Helpers;
    using UniversityAPI.Utility;
    using UniversityAPI.Utility.Helpers;
    using UniversityAPI.Utility.Interfaces;

    public abstract class BaseUnitTestFixture : IDisposable
    {
        public ApplicationDbContext Context { get; }

        public UserManager<UserDM> UserManager { get; }

        public RoleManager<IdentityRole> RoleManager { get; }

        public IConfigHelper ConfigHelper { get; }

        private readonly SqliteConnection connection;

        protected BaseUnitTestFixture()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                                   .AddJsonFile("appsettings.Development.json", optional: true)
                                                   .AddUserSecrets<Program>()
                                                   .Build();
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
            });

            var logger = loggerFactory.CreateLogger<IConfiguration>();
            this.ConfigHelper = new ConfigHelper(config, logger);
            this.ConfigHelper.InjectStaticConfig();

            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            this.Context = new ApplicationDbContext(options);
            this.Context.Database.EnsureCreated();

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

            SeedData();
        }

        protected virtual void SeedData() => TestDataSeeder.SeedDataAsync(this.Context, this.RoleManager, this.UserManager).Wait();

        public void Dispose()
        {
            this.Context.Dispose();
            connection.Close();
            connection.Dispose();
        }
    }
}