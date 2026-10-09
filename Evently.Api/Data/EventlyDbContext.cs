using Evently.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Data
{
    public class EventlyDbContext : DbContext
    {
        public EventlyDbContext(
            DbContextOptions<EventlyDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<EventCategory> EventCategories { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<PasswordResetToken>
            PasswordResetTokens
        { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(
                modelBuilder);

            // USER

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // EVENT

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // TICKET

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.Code)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new
                {
                    t.EventId,
                    t.UserId
                })
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Event)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PASSWORD RESET TOKEN

            modelBuilder
                .Entity<PasswordResetToken>()
                .HasIndex(t => t.TokenHash)
                .IsUnique();

            modelBuilder
                .Entity<PasswordResetToken>()
                .Property(t => t.TokenHash)
                .HasMaxLength(64);

            modelBuilder
                .Entity<PasswordResetToken>()
                .HasOne(t => t.User)
                .WithMany(u =>
                    u.PasswordResetTokens)
                .HasForeignKey(t =>
                    t.UserId)
                .OnDelete(
                    DeleteBehavior.Cascade);
        }
    }
}