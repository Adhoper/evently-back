namespace Evently.Api.DTOs.Tickets
{
    public class CheckInResultDto
    {
        public int TicketId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CheckedInAt { get; set; }

        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public string AttendeeName { get; set; } = string.Empty;

        public string AttendeeEmail { get; set; } = string.Empty;
    }
}