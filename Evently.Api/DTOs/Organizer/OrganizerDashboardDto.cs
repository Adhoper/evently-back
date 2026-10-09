namespace Evently.Api.DTOs.Organizer
{
    public class OrganizerDashboardDto
    {
        public int TotalEvents { get; set; }

        public int PublishedEvents { get; set; }

        public int DraftEvents { get; set; }

        public int CancelledEvents { get; set; }

        public int FinishedEvents { get; set; }

        public int TotalReservations { get; set; }

        public int TotalCheckIns { get; set; }

        public int TotalCapacity { get; set; }

        public double AttendanceRate { get; set; }

        public double OccupancyRate { get; set; }

        public TopEventDto? TopEvent { get; set; }
    }
}