using JobSeeker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }
        public DbSet<EmployerProfile> EmployerProfiles { get; set; }
        public DbSet<ProfileView> ProfileViews { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique constraint on UserId in JobSeekerProfile
            builder.Entity<JobSeekerProfile>()
                .HasIndex(j => j.UserId)
                .IsUnique();

            // Unique constraint on UserId in EmployerProfile
            builder.Entity<EmployerProfile>()
                .HasIndex(e => e.UserId)
                .IsUnique();

            // Index for faster notification queries
            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            // Index for profile views queries
            builder.Entity<ProfileView>()
                .HasIndex(p => new { p.JobSeekerProfileId, p.ViewedAt });

            // Avoid multiple cascade paths - one-to-one relationships
            builder.Entity<EmployerProfile>()
                .HasOne(e => e.User)
                .WithOne(u => u.EmployerProfile)
                .HasForeignKey<EmployerProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JobSeekerProfile>()
                .HasOne(j => j.User)
                .WithOne(u => u.JobSeekerProfile)
                .HasForeignKey<JobSeekerProfile>(j => j.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProfileView>()
                .HasOne(p => p.EmployerProfile)
                .WithMany(e => e.ProfileViews)
                .HasForeignKey(p => p.EmployerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProfileView>()
                .HasOne(p => p.JobSeekerProfile)
                .WithMany(j => j.ProfileViews)
                .HasForeignKey(p => p.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
