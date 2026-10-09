using Evently.Api.DTOs.Tickets;
using Evently.Api.Services.Results;

namespace Evently.Api.Services.Interfaces
{
    public interface ITicketService
    {
        Task<List<TicketDto>> GetMineAsync(
            int userId);

        Task<ServiceResult<TicketDto>> GetMineByIdAsync(
            int ticketId,
            int userId);

        Task<ServiceResult<TicketDto>> ReserveAsync(
            int eventId,
            int userId);

        Task<ServiceResult<TicketDto>> CancelAsync(
            int ticketId,
            int userId);

        Task<ServiceResult<CheckInResultDto>> CheckInAsync(
            string code,
            int organizerId);
    }
}