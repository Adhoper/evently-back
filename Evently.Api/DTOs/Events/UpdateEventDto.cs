using System.ComponentModel.DataAnnotations;

namespace Evently.Api.DTOs.Events
{
    public class UpdateEventDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(1, 100000)]
        public int Capacity { get; set; }

        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int EventCategoryId { get; set; }
    }
}