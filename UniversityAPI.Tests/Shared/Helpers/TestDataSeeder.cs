using Microsoft.AspNetCore.Identity;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Tests.Shared.Helpers
{
    public static class TestDataSeeder
    {
        public static async Task SeedDataAsync(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<UserDM> userManager)
        {
            await SeedRolesAsync(roleManager);

            var adminUser = await SeedAdminUserAsync(userManager);

            var testUser1 = await SeedUserAsync(userManager, "testuser1@example.com", "TestUser1!@@123");
            var testUser2 = await SeedUserAsync(userManager, "testuser2@example.com", "TestUser2!@@123");

            var universities = SeedUniversities(context, ConvertHelper.ToGuid(adminUser.Id));

            SeedBookmarks(context, ConvertHelper.ToGuid(testUser1.Id), ConvertHelper.ToGuid(testUser2.Id), universities);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
        }

        private static async Task<UserDM> SeedAdminUserAsync(UserManager<UserDM> userManager)
        {
            var adminUser = new UserDM
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!@@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            return adminUser;
        }

        private static async Task<UserDM> SeedUserAsync(UserManager<UserDM> userManager, string email, string password)
        {
            var user = new UserDM
            {
                UserName = email,
                Email = email,
                CreatedBy = Guid.Empty,
                UpdatedBy = Guid.Empty,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
            }

            return user;
        }

        private static List<UniversityDM> SeedUniversities(ApplicationDbContext context, Guid createdBy)
        {
            var universities = new List<UniversityDM>
            {
                new UniversityDM
                {
                    Id = Guid.NewGuid(),
                    Name = "Harvard University",
                    Country = "United States",
                    Webpage = "https://www.harvard.edu",
                    CreatedBy = createdBy,
                    IsActive = true
                },
                new UniversityDM
                {
                    Id = Guid.NewGuid(),
                    Name = "Stanford University",
                    Country = "United States",
                    Webpage = "https://www.stanford.edu",
                    CreatedBy = createdBy,
                    IsActive = true
                },
                new UniversityDM
                {
                    Id = Guid.NewGuid(),
                    Name = "University of Oxford",
                    Country = "United Kingdom",
                    Webpage = "https://www.ox.ac.uk",
                    CreatedBy = createdBy,
                    IsActive = true
                },
                new UniversityDM
                {
                    Id = Guid.NewGuid(),
                    Name = "ETH Zurich",
                    Country = "Switzerland",
                    Webpage = "https://www.ethz.ch",
                    CreatedBy = createdBy,
                    IsActive = true
                },
                new UniversityDM
                {
                    Id = Guid.NewGuid(),
                    Name = "University of Tokyo",
                    Country = "Japan",
                    Webpage = "https://www.u-tokyo.ac.jp",
                    CreatedBy = createdBy,
                    IsActive = true
                }
            };

            context.Universities.AddRange(universities);
            return universities;
        }

        private static void SeedBookmarks(ApplicationDbContext context, Guid userId1, Guid userId2, List<UniversityDM> universities)
        {
            var bookmarks = new List<UserBookmarkDM>
            {
                new UserBookmarkDM
                {
                    UserId = userId1,
                    UniversityId = universities[3].Id,
                    BookmarkedAt = DateTime.UtcNow.AddDays(-5),
                    CreatedBy = userId1
                },
                new UserBookmarkDM
                {
                    UserId = userId1,
                    UniversityId = universities[4].Id,
                    BookmarkedAt = DateTime.UtcNow.AddDays(-3),
                    CreatedBy = userId1
                },

                new UserBookmarkDM
                {
                    UserId = userId2,
                    UniversityId = universities[2].Id,
                    BookmarkedAt = DateTime.UtcNow.AddDays(-2),
                    CreatedBy = userId2
                },
                new UserBookmarkDM
                {
                    UserId = userId2,
                    UniversityId = universities[3].Id,
                    BookmarkedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedBy = userId2
                },
                new UserBookmarkDM
                {
                    UserId = userId2,
                    UniversityId = universities[4].Id,
                    BookmarkedAt = DateTime.UtcNow,
                    CreatedBy = userId2
                }
            };

            context.UserBookmarks.AddRange(bookmarks);
        }
    }
}