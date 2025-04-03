using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Framework
{
    public class ApplicationDbContext : IdentityDbContext<UserDM>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<UniversityDM> Universities { get; set; }
        public DbSet<UserBookmarkDM> UserBookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserBookmarkDM>()
                .HasKey(ub => new { ub.UserId, ub.UniversityId });
        }
    }
}