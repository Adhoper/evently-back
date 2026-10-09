namespace Evently.Api.DTOs.Admin
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int Organizers { get; set; }
        public int TotalEvents { get; set; }
        public int PublishedEvents { get; set; }
        public int TotalReservations { get; set; }
        public int TotalCheckIns { get; set; }
        public int TotalCategories { get; set; }
    }
}
