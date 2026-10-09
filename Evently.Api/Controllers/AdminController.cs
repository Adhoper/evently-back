using Evently.Api.DTOs.Admin;
using Evently.Api.DTOs.Categories;
using Evently.Api.Extensions;
using Evently.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Evently.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
        {
            return Ok(await _adminService.GetDashboardAsync());
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
        {
            return Ok(await _adminService.GetUsersAsync());
        }

        [HttpPatch("users/{userId:int}/status")]
        public async Task<ActionResult<AdminUserDto>> UpdateUserStatus(
            int userId,
            UpdateUserStatusDto dto)
        {
            var result = await _adminService.UpdateUserStatusAsync(
                userId,
                dto.IsActive,
                User.GetUserId());

            if (!result.Success)
            {
                return result.NotFound
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpPatch("users/{userId:int}/role")]
        public async Task<ActionResult<AdminUserDto>> UpdateUserRole(
            int userId,
            UpdateUserRoleDto dto)
        {
            var result = await _adminService.UpdateUserRoleAsync(
                userId,
                dto.Role,
                User.GetUserId());

            if (!result.Success)
            {
                return result.NotFound
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpGet("events")]
        public async Task<ActionResult<List<AdminEventDto>>> GetEvents()
        {
            return Ok(await _adminService.GetEventsAsync());
        }

        [HttpPatch("events/{eventId:int}/cancel")]
        public async Task<ActionResult<AdminEventDto>> CancelEvent(int eventId)
        {
            var result = await _adminService.CancelEventAsync(eventId);

            if (!result.Success)
            {
                return result.NotFound
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories()
        {
            return Ok(await _adminService.GetCategoriesAsync());
        }

        [HttpPost("categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto dto)
        {
            var result = await _adminService.CreateCategoryAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpPut("categories/{categoryId:int}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(
            int categoryId,
            UpdateCategoryDto dto)
        {
            var result = await _adminService.UpdateCategoryAsync(categoryId, dto);

            if (!result.Success)
            {
                return result.NotFound
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpPatch("categories/{categoryId:int}/status")]
        public async Task<ActionResult<CategoryDto>> UpdateCategoryStatus(
            int categoryId,
            UpdateCategoryStatusDto dto)
        {
            var result = await _adminService.UpdateCategoryStatusAsync(categoryId, dto.IsActive);

            if (!result.Success)
            {
                return result.NotFound
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
    }
}
