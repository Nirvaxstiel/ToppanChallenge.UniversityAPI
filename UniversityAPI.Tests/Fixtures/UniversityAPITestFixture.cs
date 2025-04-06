using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Tests
{
    public class UniversityAPITestFixture : IDisposable
    {
        public ApplicationDbContext Context { get; }
        public UserManager<UserDM> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }

        public UniversityAPITestFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new ApplicationDbContext(options);
            var userStore = new UserStore<UserDM>(Context);
            UserManager = new UserManager<UserDM>(userStore, null, new PasswordHasher<UserDM>(), null, null, null, null, null, null);

            var roleStore = new RoleStore<IdentityRole>(Context);
            RoleManager = new RoleManager<IdentityRole>(roleStore, null, null, null, null);

            TestDataSeeder.SeedDataAsync(Context, RoleManager, UserManager).Wait();
        }

        public void Dispose() => Context.Dispose();
    }
}