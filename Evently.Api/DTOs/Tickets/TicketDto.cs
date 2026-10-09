namespace Evently.Api.DTOs.Tickets
{
    public class TicketDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime ReservedAt { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public TimeSpan EventStartTime { get; set; }

        public string EventLocation { get; set; } = string.Empty;

        public string? EventImageUrl { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}