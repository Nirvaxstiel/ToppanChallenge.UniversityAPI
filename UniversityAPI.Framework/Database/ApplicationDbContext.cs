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

        public DbSet<UserDM> Users { get; set; }
        public DbSet<UniversityDM> Universities { get; set; }
        public DbSet<UserBookmarkDM> UserBookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserDM>().ToTable("Users");

            builder.Entity<UserDM>(b =>
            {
                b.Property(u => u.CreatedDate).HasColumnName("CreatedDate");
                b.Property(u => u.UpdatedDate).HasColumnName("UpdatedDate");
                b.Property(u => u.CreatedBy).HasColumnName("CreatedBy");
                b.Property(u => u.UpdatedBy).HasColumnName("UpdatedBy");
                b.Property(u => u.IsActive).HasColumnName("IsActive");
                b.HasQueryFilter(item => item.IsActive);
            });

            builder.Entity<UserBookmarkDM>()
                .HasQueryFilter(item => item.IsActive)
                .HasKey(ub => new { ub.UserId, ub.UniversityId });

            builder.Entity<UniversityDM>()
                .HasQueryFilter(item => item.IsActive);
        }
    }
}