namespace Evently.Api.Models
{
    public class EventCategory
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}