namespace Evently.Api.DTOs.Admin
{
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EventsCount { get; set; }
        public int TicketsCount { get; set; }
    }
}
