using Evently.Api.DTOs.Auth;
using Evently.Api.Services.Results;

namespace Evently.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(int userId);

        Task<ServiceResult<AuthResponseDto>> BecomeOrganizerAsync(
            int userId);
    }
}