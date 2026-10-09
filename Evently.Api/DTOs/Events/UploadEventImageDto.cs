using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Evently.Api.DTOs.Events
{
    public class UploadEventImageDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}