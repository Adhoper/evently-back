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
                return NotFound();
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
                    message = "La categoría seleccionada no existe o está inactiva."
                });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = eventItem.Id },
                eventItem);
        }
    }
}