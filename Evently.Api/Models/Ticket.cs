using Evently.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Evently.Api.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string Code { get; set; } = string.Empty;

        public TicketStatus Status { get; set; }
            = TicketStatus.Reserved;

        public DateTime ReservedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? CheckedInAt { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = null!;

        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}