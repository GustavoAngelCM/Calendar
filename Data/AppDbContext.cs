using Microsoft.EntityFrameworkCore;
using Calendar.Models;

namespace Calendar.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Participation> Participations => Set<Participation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Participation>()
                .HasOne(p => p.User)
                .WithMany(u => u.Participations)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Participation>()
                .HasOne(p => p.Event)
                .WithMany(e => e.Participations)
                .HasForeignKey(p => p.EventId);

            modelBuilder.Entity<Participation>()
                .HasIndex(p => new { p.UserId, p.EventId })
                .IsUnique();
        }
    }
}