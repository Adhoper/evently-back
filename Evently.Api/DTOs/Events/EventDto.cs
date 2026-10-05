namespace Evently.Api.DTOs.Events
{
    public class EventDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public string Location { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public string? ImageUrl { get; set; }

        public string Status { get; set; } = string.Empty;

        public int EventCategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}