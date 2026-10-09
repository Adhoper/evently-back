using Evently.Api.Services.Results;
using Microsoft.AspNetCore.Http;

namespace Evently.Api.Services.Interfaces
{
    public interface IEventImageService
    {
        Task<ServiceResult<string>> UploadAsync(
            int eventId,
            int organizerId,
            IFormFile file);

        Task<ServiceResult<string>> RemoveAsync(
            int eventId,
            int organizerId);
    }
}