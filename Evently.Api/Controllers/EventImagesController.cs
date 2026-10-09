using Evently.Api.DTOs.Events;
using Evently.Api.Extensions;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Evently.Api.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:int}/image")]
    [Authorize(Roles = "Organizer")]
    public class EventImagesController : ControllerBase
    {
        private readonly IEventImageService _eventImageService;

        public EventImagesController(
            IEventImageService eventImageService)
        {
            _eventImageService = eventImageService;
        }

        // =====================================================
        // UPLOAD / REPLACE
        // =====================================================

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Upload(
            int eventId,
            [FromForm] UploadEventImageDto dto)
        {
            var organizerId =
                User.GetUserId();

            var result =
                await _eventImageService
                    .UploadAsync(
                        eventId,
                        organizerId,
                        dto.File);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(
                        new
                        {
                            message =
                                result.Message
                        });
                }

                return BadRequest(
                    new
                    {
                        message =
                            result.Message
                    });
            }

            return Ok(
                new
                {
                    imageUrl =
                        result.Data
                });
        }

        // =====================================================
        // REMOVE
        // =====================================================

        [HttpDelete]
        public async Task<IActionResult> Remove(
            int eventId)
        {
            var organizerId =
                User.GetUserId();

            var result =
                await _eventImageService
                    .RemoveAsync(
                        eventId,
                        organizerId);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(
                        new
                        {
                            message =
                                result.Message
                        });
                }

                return BadRequest(
                    new
                    {
                        message =
                            result.Message
                    });
            }

            return NoContent();
        }
    }
}