using Evently.Api.Data;
using Evently.Api.DTOs.Admin;
using Evently.Api.DTOs.Categories;
using Evently.Api.Models;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Evently.Api.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly EventlyDbContext _context;

        public AdminService(EventlyDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var organizers = await _context.Users.CountAsync(u => u.Role == UserRole.Organizer && u.IsActive);
            var totalEvents = await _context.Events.CountAsync();
            var publishedEvents = await _context.Events.CountAsync(e => e.Status == EventStatus.Published);
            var totalReservations = await _context.Tickets.CountAsync(t => t.Status != TicketStatus.Cancelled);
            var totalCheckIns = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.CheckedIn);
            var totalCategories = await _context.EventCategories.CountAsync(c => c.IsActive);

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                Organizers = organizers,
                TotalEvents = totalEvents,
                PublishedEvents = publishedEvents,
                TotalReservations = totalReservations,
                TotalCheckIns = totalCheckIns,
                TotalCategories = totalCategories
            };
        }

        public async Task<List<AdminUserDto>> GetUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    EventsCount = u.Events.Count(),
                    TicketsCount = u.Tickets.Count(t => t.Status != TicketStatus.Cancelled)
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<AdminUserDto>> UpdateUserStatusAsync(
            int userId,
            bool isActive,
            int currentAdminId)
        {
            if (userId == currentAdminId)
            {
                return ServiceResult<AdminUserDto>.Failure("No puedes desactivar tu propia cuenta administradora.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return ServiceResult<AdminUserDto>.Missing("El usuario no existe.");
            }

            if (user.Role == UserRole.Admin)
            {
                return ServiceResult<AdminUserDto>.Failure("Las cuentas administradoras no se modifican desde este panel.");
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();

            return ServiceResult<AdminUserDto>.Ok(await MapUserAsync(user.Id));
        }

        public async Task<ServiceResult<AdminUserDto>> UpdateUserRoleAsync(
            int userId,
            string role,
            int currentAdminId)
        {
            if (userId == currentAdminId)
            {
                return ServiceResult<AdminUserDto>.Failure("No puedes cambiar tu propio rol administrador.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return ServiceResult<AdminUserDto>.Missing("El usuario no existe.");
            }

            if (user.Role == UserRole.Admin)
            {
                return ServiceResult<AdminUserDto>.Failure("Las cuentas administradoras no se modifican desde este panel.");
            }

            if (!Enum.TryParse<UserRole>(role, true, out var parsedRole) ||
                parsedRole == UserRole.Admin)
            {
                return ServiceResult<AdminUserDto>.Failure("El rol debe ser User u Organizer.");
            }

            user.Role = parsedRole;
            await _context.SaveChangesAsync();

            return ServiceResult<AdminUserDto>.Ok(await MapUserAsync(user.Id));
        }

        public async Task<List<AdminEventDto>> GetEventsAsync()
        {
            return await _context.Events
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new AdminEventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    StartTime = e.StartTime,
                    Location = e.Location,
                    Capacity = e.Capacity,
                    Status = e.Status.ToString(),
                    CategoryName = e.EventCategory.Name,
                    OrganizerName = e.Organizer.FirstName + " " + e.Organizer.LastName,
                    OrganizerEmail = e.Organizer.Email,
                    Reservations = e.Tickets.Count(t => t.Status != TicketStatus.Cancelled),
                    CheckIns = e.Tickets.Count(t => t.Status == TicketStatus.CheckedIn),
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<AdminEventDto>> CancelEventAsync(int eventId)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity is null)
            {
                return ServiceResult<AdminEventDto>.Missing("El evento no existe.");
            }

            if (eventEntity.Status != EventStatus.Published)
            {
                return ServiceResult<AdminEventDto>.Failure("Solo se pueden cancelar eventos publicados.");
            }

            eventEntity.Status = EventStatus.Cancelled;

            foreach (var ticket in eventEntity.Tickets.Where(t => t.Status == TicketStatus.Reserved))
            {
                ticket.Status = TicketStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            var updatedEvent = (await GetEventsAsync()).First(e => e.Id == eventId);
            return ServiceResult<AdminEventDto>.Ok(updatedEvent);
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            return await _context.EventCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var name = dto.Name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return ServiceResult<CategoryDto>.Failure("El nombre de la categoría es obligatorio.");
            }

            var exists = await _context.EventCategories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return ServiceResult<CategoryDto>.Failure("Ya existe una categoría con ese nombre.");
            }

            var category = new EventCategory
            {
                Name = name,
                IsActive = true
            };

            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            return ServiceResult<CategoryDto>.Ok(MapCategory(category));
        }

        public async Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(
            int categoryId,
            UpdateCategoryDto dto)
        {
            var category = await _context.EventCategories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category is null)
            {
                return ServiceResult<CategoryDto>.Missing("La categoría no existe.");
            }

            var name = dto.Name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return ServiceResult<CategoryDto>.Failure("El nombre de la categoría es obligatorio.");
            }

            var exists = await _context.EventCategories
                .AnyAsync(c => c.Id != categoryId && c.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return ServiceResult<CategoryDto>.Failure("Ya existe otra categoría con ese nombre.");
            }

            category.Name = name;
            await _context.SaveChangesAsync();

            return ServiceResult<CategoryDto>.Ok(MapCategory(category));
        }

        public async Task<ServiceResult<CategoryDto>> UpdateCategoryStatusAsync(
            int categoryId,
            bool isActive)
        {
            var category = await _context.EventCategories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category is null)
            {
                return ServiceResult<CategoryDto>.Missing("La categoría no existe.");
            }

            category.IsActive = isActive;
            await _context.SaveChangesAsync();

            return ServiceResult<CategoryDto>.Ok(MapCategory(category));
        }

        private async Task<AdminUserDto> MapUserAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    EventsCount = u.Events.Count(),
                    TicketsCount = u.Tickets.Count(t => t.Status != TicketStatus.Cancelled)
                })
                .FirstAsync();
        }

        private static CategoryDto MapCategory(EventCategory category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive
            };
        }
    }
}
