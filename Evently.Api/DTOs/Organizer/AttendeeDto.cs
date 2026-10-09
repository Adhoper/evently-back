namespace Evently.Api.DTOs.Organizer
{
    public class AttendeeDto
    {
        public int TicketId { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Status { get; set; }
            = string.Empty;

        public DateTime ReservedAt { get; set; }

        public DateTime? CheckedInAt { get; set; }
    }
}