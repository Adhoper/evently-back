using Evently.Api.DTOs.Events;
using Evently.Api.Extensions;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Evently.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(
            IEventService eventService)
        {
            _eventService = eventService;
        }
        [HttpGet]
        public async Task<ActionResult<List<EventDto>>> GetAll()
        {
            var events =
                await _eventService.GetPublicAsync();

            return Ok(events);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventDetailDto>> GetById(
            int id)
        {
            var eventItem =
                await _eventService.GetPublicByIdAsync(id);

            if (eventItem is null)
            {
                return NotFound(new
                {
                    message = "El evento no existe."
                });
            }

            return Ok(eventItem);
        }
        [HttpGet("mine")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<List<EventDto>>> GetMine()
        {
            var organizerId =
                User.GetUserId();

            var events =
                await _eventService.GetMineAsync(
                    organizerId);

            return Ok(events);
        }
        [HttpGet("mine/{id:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<EventDetailDto>>
            GetMineById(int id)
        {
            var organizerId =
                User.GetUserId();

            var eventItem =
                await _eventService.GetMineByIdAsync(
                    id,
                    organizerId);

            if (eventItem is null)
            {
                return NotFound(new
                {
                    message =
                        "El evento no existe o no pertenece al usuario."
                });
            }

            return Ok(eventItem);
        }

        [HttpPost]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<EventDetailDto>> Create(
            CreateEventDto dto)
        {
            var organizerId =
                User.GetUserId();

            var eventItem =
                await _eventService.CreateAsync(
                    dto,
                    organizerId);

            if (eventItem is null)
            {
                return BadRequest(new
                {
                    message =
                        "La categoría seleccionada no existe o está inactiva."
                });
            }
            return CreatedAtAction(
                nameof(GetMineById),
                new { id = eventItem.Id },
                eventItem);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<EventDetailDto>> Update(
            int id,
            UpdateEventDto dto)
        {
            var organizerId =
                User.GetUserId();

            var result =
                await _eventService.UpdateAsync(
                    id,
                    dto,
                    organizerId);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(new
                    {
                        message = result.Message
                    });
                }

                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Data);
        }

        [HttpPatch("{id:int}/publish")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<EventDetailDto>> Publish(
            int id)
        {
            var organizerId =
                User.GetUserId();

            var result =
                await _eventService.PublishAsync(
                    id,
                    organizerId);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(new
                    {
                        message = result.Message
                    });
                }

                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Data);
        }

        [HttpPatch("{id:int}/cancel")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<EventDetailDto>> Cancel(
            int id)
        {
            var organizerId =
                User.GetUserId();

            var result =
                await _eventService.CancelAsync(
                    id,
                    organizerId);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(new
                    {
                        message = result.Message
                    });
                }

                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Data);
        }
    }
}