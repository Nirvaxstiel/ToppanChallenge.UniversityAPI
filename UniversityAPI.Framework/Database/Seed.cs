namespace UniversityAPI.Framework.Database
{
    using System.Text.Json;
    using Microsoft.AspNetCore.Identity;
    using UniversityAPI.Framework.Model.University;
    using UniversityAPI.Framework.Model.User;
    using UniversityAPI.Utility.Interfaces;

    public static class Seed
    {
        private const string UNABLETOSEEDADMINMESSAGE = "Admin username, email, or password not set. Set TOPPAN_UNIVERSITYAPI_ADMIN_INIT_USERNAME, TOPPAN_UNIVERSITYAPI_ADMIN_INIT_EMAIL, TOPPAN_UNIVERSITYAPI_ADMIN_INIT_PASSWORD in dotnet secrets or corresponding environment variables.";
        private static IConfigHelper? configHelper;

        public static async Task SeedData(ApplicationDbContext context,
                                          UserManager<UserDM> userManager,
                                          RoleManager<IdentityRole> roleManager,
                                          IConfigHelper configHelper)
        {
            await context.Database.EnsureCreatedAsync();
            Seed.configHelper = configHelper;
            await SeedRoles(roleManager);
            await SeedAdminUser(userManager);
            await SeedUniversities(context);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = ["Admin", "User"];

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<UserDM> userManager)
        {
            var adminUsername = configHelper.GetAdminInitUsername<string>();
            var adminEmail = configHelper.GetAdminInitUsername<string>();
            var adminPassword = configHelper.GetAdminInitPassword<string>();

            if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new ArgumentException(message: UNABLETOSEEDADMINMESSAGE);
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new UserDM
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createUser = await userManager.CreateAsync(newAdmin, adminPassword);

                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        private static async Task SeedUniversities(ApplicationDbContext context)
        {
            if (!context.Universities.Any())
            {
                var universities = new List<UniversityDM>
            {
                new() {
                    Name = "Harvard University",
                    Country = "United States",
                    Webpage = "https://www.harvard.edu",
                    IsActive = true
                },
                new() {
                    Name = "Stanford University",
                    Country = "United States",
                    Webpage = "https://www.stanford.edu",
                    IsActive = true
                },
                new() {
                    Name = "University of Oxford",
                    Country = "United Kingdom",
                    Webpage = "https://www.ox.ac.uk",
                    IsActive = true
                }
            };

                await context.Universities.AddRangeAsync(universities);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBookmarks(ApplicationDbContext context)
        {
            if (!context.UserBookmarks.Any())
            {
                var bookmarks = new List<UserBookmarkDM>
                {
                    new() {
                    }
                };

                await context.UserBookmarks.AddRangeAsync(bookmarks);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedFromJson(ApplicationDbContext context)
        {
            if (!context.Universities.Any())
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "universities.json");
                var jsonData = await File.ReadAllTextAsync(filePath);
                var universities = JsonSerializer.Deserialize<List<UniversityDM>>(jsonData);

                await context.Universities.AddRangeAsync(universities);
                await context.SaveChangesAsync();
            }
        }
    }
}
