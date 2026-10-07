using System.ComponentModel.DataAnnotations;

namespace Evently.Api.DTOs.Tickets
{
    public class CheckInTicketDto
    {
        [Required]
        [MaxLength(64)]
        public string Code { get; set; } = string.Empty;
    }
}