using Evently.Api.DTOs.Organizer;
using Evently.Api.Services.Results;

namespace Evently.Api.Services.Interfaces
{
    public interface IOrganizerService
    {
        Task<OrganizerDashboardDto> GetDashboardAsync(
            int organizerId);

        Task<ServiceResult<EventAttendeesDto>> GetEventAttendeesAsync(
            int eventId,
            int organizerId);
    }
}