using Evently.Api.DTOs.Tickets;
using Evently.Api.Extensions;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Evently.Api.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(
            ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("mine")]
        public async Task<ActionResult<List<TicketDto>>> GetMine()
        {
            var userId =
                User.GetUserId();

            var tickets =
                await _ticketService.GetMineAsync(
                    userId);

            return Ok(tickets);
        }

        [HttpGet("mine/{id:int}")]
        public async Task<ActionResult<TicketDto>> GetMineById(
            int id)
        {
            var userId =
                User.GetUserId();

            var result =
                await _ticketService.GetMineByIdAsync(
                    id,
                    userId);

            if (!result.Success)
            {
                return NotFound(new
                {
                    message =
                        result.Message
                });
            }

            return Ok(result.Data);
        }

        [HttpPost("events/{eventId:int}/reserve")]
        public async Task<ActionResult<TicketDto>> Reserve(
            int eventId)
        {
            var userId =
                User.GetUserId();

            var result =
                await _ticketService.ReserveAsync(
                    eventId,
                    userId);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(new
                    {
                        message =
                            result.Message
                    });
                }

                return BadRequest(new
                {
                    message =
                        result.Message
                });
            }

            return Ok(result.Data);
        }

        [HttpPatch("{id:int}/cancel")]
        public async Task<ActionResult<TicketDto>> Cancel(
            int id)
        {
            var userId =
                User.GetUserId();

            var result =
                await _ticketService.CancelAsync(
                    id,
                    userId);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(new
                    {
                        message =
                            result.Message
                    });
                }

                return BadRequest(new
                {
                    message =
                        result.Message
                });
            }

            return Ok(result.Data);
        }

        [HttpPost("check-in")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<CheckInResultDto>> CheckIn(
            CheckInTicketDto dto)
        {
            var organizerId =
                User.GetUserId();

            var result =
                await _ticketService.CheckInAsync(
                    dto.Code,
                    organizerId);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(new
                    {
                        message =
                            result.Message
                    });
                }

                return BadRequest(new
                {
                    message =
                        result.Message
                });
            }

            return Ok(result.Data);
        }
    }
}