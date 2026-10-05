using Evently.Api.DTOs.Events;

namespace Evently.Api.Services.Interfaces
{
    public interface IEventService
    {
        Task<List<EventDto>> GetAllAsync();

        Task<EventDetailDto?> GetByIdAsync(int id);

        Task<EventDetailDto?> CreateAsync(CreateEventDto dto);
    }
}