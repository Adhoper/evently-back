using Evently.Api.DTOs.Events;
using Evently.Api.Services.Results;

namespace Evently.Api.Services.Interfaces
{
    public interface IEventService
    {
        Task<List<EventDto>> GetAllAsync();

        Task<EventDetailDto?> GetByIdAsync(int id);

        Task<EventDetailDto?> CreateAsync(CreateEventDto dto);

        Task<ServiceResult<EventDetailDto>> UpdateAsync(
            int id,
            UpdateEventDto dto);

        Task<ServiceResult<EventDetailDto>> PublishAsync(int id);

        Task<ServiceResult<EventDetailDto>> CancelAsync(int id);
    }
}