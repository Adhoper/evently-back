using Evently.Api.DTOs.Admin;
using Evently.Api.DTOs.Categories;
using Evently.Api.Services.Results;

namespace Evently.Api.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<List<AdminUserDto>> GetUsersAsync();
        Task<ServiceResult<AdminUserDto>> UpdateUserStatusAsync(int userId, bool isActive, int currentAdminId);
        Task<ServiceResult<AdminUserDto>> UpdateUserRoleAsync(int userId, string role, int currentAdminId);
        Task<List<AdminEventDto>> GetEventsAsync();
        Task<ServiceResult<AdminEventDto>> CancelEventAsync(int eventId);
        Task<List<CategoryDto>> GetCategoriesAsync();
        Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto);
        Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(int categoryId, UpdateCategoryDto dto);
        Task<ServiceResult<CategoryDto>> UpdateCategoryStatusAsync(int categoryId, bool isActive);
    }
}
