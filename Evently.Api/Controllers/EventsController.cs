using Evently.Api.DTOs.Events;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Evently.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EventDto>>> GetAll()
        {
            var events = await _eventService.GetAllAsync();

            return Ok(events);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventDetailDto>> GetById(int id)
        {
            var eventItem = await _eventService.GetByIdAsync(id);

            if (eventItem is null)
            {
                return NotFound(new
                {
                    message = "El evento no existe."
                });
            }

            return Ok(eventItem);
        }

        [HttpPost]
        public async Task<ActionResult<EventDetailDto>> Create(
            CreateEventDto dto)
        {
            var eventItem = await _eventService.CreateAsync(dto);

            if (eventItem is null)
            {
                return BadRequest(new
                {
                    message =
                        "La categoría seleccionada no existe o está inactiva."
                });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = eventItem.Id },
                eventItem);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EventDetailDto>> Update(
            int id,
            UpdateEventDto dto)
        {
            var result = await _eventService.UpdateAsync(id, dto);

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
        public async Task<ActionResult<EventDetailDto>> Publish(int id)
        {
            var result = await _eventService.PublishAsync(id);

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
        public async Task<ActionResult<EventDetailDto>> Cancel(int id)
        {
            var result = await _eventService.CancelAsync(id);

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