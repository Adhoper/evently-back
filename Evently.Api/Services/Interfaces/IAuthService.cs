using Evently.Api.DTOs.Auth;
using Evently.Api.DTOs.Common;
using Evently.Api.Services.Results;

namespace Evently.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<AuthResponseDto>>
            RegisterAsync(
                RegisterDto dto);

        Task<ServiceResult<AuthResponseDto>>
            LoginAsync(
                LoginDto dto);

        Task<ServiceResult<MessageDto>>
            ForgotPasswordAsync(
                ForgotPasswordDto dto);

        Task<ServiceResult<MessageDto>>
            ResetPasswordAsync(
                ResetPasswordDto dto);
    }
}