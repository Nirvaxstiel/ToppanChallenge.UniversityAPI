using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Framework
{
    public static class Seed
    {
        public static async Task SeedData(ApplicationDbContext context,
                                          UserManager<UserDM> userManager,
                                          RoleManager<IdentityRole> roleManager)
        {
            await context.Database.EnsureCreatedAsync();
            await SeedRoles(roleManager);
            await SeedAdminUser(userManager);
            await SeedUniversities(context);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };

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
            var adminEmail = "admin@university.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new UserDM
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                string adminPassword = "Admin@1234";
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
                new UniversityDM
                {
                    Name = "Harvard University",
                    Country = "United States",
                    Webpage = "https://www.harvard.edu",
                    IsActive = true
                },
                new UniversityDM
                {
                    Name = "Stanford University",
                    Country = "United States",
                    Webpage = "https://www.stanford.edu",
                    IsActive = true
                },
                new UniversityDM
                {
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
                    new UserBookmarkDM
                    {
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