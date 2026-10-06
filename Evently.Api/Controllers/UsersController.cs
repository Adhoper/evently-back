using Evently.Api.DTOs.Auth;
using Evently.Api.Extensions;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Evently.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(
            IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            var userId = User.GetUserId();

            var user =
                await _userService.GetByIdAsync(userId);

            if (user is null)
            {
                return NotFound(new
                {
                    message = "El usuario no existe."
                });
            }

            return Ok(user);
        }

        [HttpPost("become-organizer")]
        public async Task<ActionResult<AuthResponseDto>>
            BecomeOrganizer()
        {
            var userId = User.GetUserId();

            var result =
                await _userService
                    .BecomeOrganizerAsync(userId);

            if (!result.Success)
            {
                if (result.NotFound)
                {
                    return NotFound(new
                    {
                        message = result.Message
                    });
                }

                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Data);
        }
    }
}