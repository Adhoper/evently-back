using Evently.Api.DTOs.Auth;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Evently.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(
            RegisterDto dto)
        {
            var result =
                await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Data);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(
            LoginDto dto)
        {
            var result =
                await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                return Unauthorized(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Data);
        }
    }
}