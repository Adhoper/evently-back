namespace Evently.Api.DTOs.Organizer
{
    public class TopEventDto
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public int Reservations { get; set; }

        public int CheckIns { get; set; }

        public int Capacity { get; set; }

        public double OccupancyRate { get; set; }
    }
}