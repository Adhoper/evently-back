using Evently.Api.Data;
using Evently.Api.DTOs.Events;
using Evently.Api.Models;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Services
{
    public class EventService : IEventService
    {
        private readonly EventlyDbContext _context;

        public EventService(EventlyDbContext context)
        {
            _context = context;
        }

        public async Task<List<EventDto>> GetAllAsync()
        {
            return await _context.Events
                .AsNoTracking()
                .OrderBy(e => e.Date)
                .ThenBy(e => e.StartTime)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    StartTime = e.StartTime,
                    Location = e.Location,
                    Capacity = e.Capacity,
                    ImageUrl = e.ImageUrl,
                    Status = e.Status.ToString(),

                    EventCategoryId = e.EventCategoryId,
                    CategoryName = e.EventCategory.Name
                })
                .ToListAsync();
        }

        public async Task<EventDetailDto?> GetByIdAsync(int id)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new EventDetailDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    StartTime = e.StartTime,
                    Location = e.Location,
                    Capacity = e.Capacity,
                    ImageUrl = e.ImageUrl,
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt,

                    EventCategoryId = e.EventCategoryId,
                    CategoryName = e.EventCategory.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<EventDetailDto?> CreateAsync(CreateEventDto dto)
        {
            var categoryExists = await _context.EventCategories
                .AnyAsync(c =>
                    c.Id == dto.EventCategoryId &&
                    c.IsActive);

            if (!categoryExists)
            {
                return null;
            }

            var eventEntity = new Event
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Date = dto.Date.Date,
                StartTime = dto.StartTime,
                Location = dto.Location.Trim(),
                Capacity = dto.Capacity,
                ImageUrl = dto.ImageUrl,
                Status = EventStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                EventCategoryId = dto.EventCategoryId
            };

            _context.Events.Add(eventEntity);

            await _context.SaveChangesAsync();

            return await GetByIdAsync(eventEntity.Id);
        }
    }
}