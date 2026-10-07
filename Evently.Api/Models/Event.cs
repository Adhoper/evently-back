using Evently.Api.Models.Enums;

namespace Evently.Api.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public string Location { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public string? ImageUrl { get; set; }

        public EventStatus Status { get; set; } = EventStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int EventCategoryId { get; set; }

        public EventCategory EventCategory { get; set; } = null!;

        public int OrganizerId { get; set; }

        public User Organizer { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; }
    = new List<Ticket>();
    }
}