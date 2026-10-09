using Evently.Api.DTOs.Organizer;
using Evently.Api.Extensions;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Evently.Api.Controllers
{
    [ApiController]
    [Route("api/organizer")]
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : ControllerBase
    {
        private readonly IOrganizerService _organizerService;

        public OrganizerController(
            IOrganizerService organizerService)
        {
            _organizerService =
                organizerService;
        }

        // =====================================================
        // DASHBOARD
        // =====================================================

        [HttpGet("dashboard")]
        public async Task<ActionResult<OrganizerDashboardDto>>
            GetDashboard()
        {
            var organizerId =
                User.GetUserId();

            var dashboard =
                await _organizerService
                    .GetDashboardAsync(
                        organizerId);

            return Ok(dashboard);
        }

        // =====================================================
        // EVENT ATTENDEES
        // =====================================================

        [HttpGet("events/{eventId:int}/attendees")]
        public async Task<ActionResult<EventAttendeesDto>>
            GetEventAttendees(
                int eventId)
        {
            var organizerId =
                User.GetUserId();

            var result =
                await _organizerService
                    .GetEventAttendeesAsync(
                        eventId,
                        organizerId);

            if (!result.Success)
            {
                return NotFound(new
                {
                    message =
                        result.Message
                });
            }

            return Ok(
                result.Data);
        }
    }
}