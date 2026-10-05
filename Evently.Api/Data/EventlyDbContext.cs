using Evently.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Data
{
    public class EventlyDbContext : DbContext
    {
        public EventlyDbContext(DbContextOptions<EventlyDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventCategory> EventCategories { get; set; }

        public DbSet<Event> Events { get; set; }
    }
}