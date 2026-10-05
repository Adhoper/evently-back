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

        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int EventCategoryId { get; set; }

        public EventCategory EventCategory { get; set; } = null!;
    }
}