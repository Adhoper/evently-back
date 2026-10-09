using Evently.Api.DTOs.Events;
using Evently.Api.Services.Results;

namespace Evently.Api.Services.Interfaces
{
    public interface IEventService
    {
        Task<List<EventDto>> GetPublicAsync();

        Task<EventDetailDto?> GetPublicByIdAsync(int id);

        Task<List<EventDto>> GetMineAsync(
            int organizerId);

        Task<EventDetailDto?> GetMineByIdAsync(
            int id,
            int organizerId);

        Task<EventDetailDto?> CreateAsync(
            CreateEventDto dto,
            int organizerId);

        Task<ServiceResult<EventDetailDto>> UpdateAsync(
            int id,
            UpdateEventDto dto,
            int organizerId);

        Task<ServiceResult<EventDetailDto>> PublishAsync(
            int id,
            int organizerId);

        Task<ServiceResult<EventDetailDto>> CancelAsync(
            int id,
            int organizerId);
    }
}