namespace Evently.Api.DTOs.Organizer
{
    public class EventAttendeesDto
    {
        public int EventId { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string Status { get; set; }
            = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public string Location { get; set; }
            = string.Empty;

        public int Capacity { get; set; }

        public int ReservedCount { get; set; }

        public int CheckedInCount { get; set; }

        public int CancelledCount { get; set; }

        public int AvailableSpots { get; set; }

        public double OccupancyRate { get; set; }

        public double AttendanceRate { get; set; }

        public List<AttendeeDto> Attendees { get; set; }
            = new();
    }
}