using Evently.Api.Data;
using Evently.Api.DTOs.Events;
using Evently.Api.Models;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Evently.Api.Services.Results;
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

        // =====================================================
        // PUBLIC
        // =====================================================

        public async Task<List<EventDto>> GetPublicAsync()
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.Status == EventStatus.Published)
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

        public async Task<EventDetailDto?> GetPublicByIdAsync(int id)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e =>
                    e.Id == id &&
                    e.Status == EventStatus.Published)
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

        // =====================================================
        // ORGANIZER - MY EVENTS
        // =====================================================

        public async Task<List<EventDto>> GetMineAsync(int organizerId)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.OrganizerId == organizerId)
                .OrderByDescending(e => e.CreatedAt)
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

        public async Task<EventDetailDto?> GetMineByIdAsync(
            int id,
            int organizerId)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e =>
                    e.Id == id &&
                    e.OrganizerId == organizerId)
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

        // =====================================================
        // CREATE
        // =====================================================

        public async Task<EventDetailDto?> CreateAsync(
            CreateEventDto dto,
            int organizerId)
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
                EventCategoryId = dto.EventCategoryId,

                // El OrganizerId viene del usuario autenticado
                // mediante el JWT.
                OrganizerId = organizerId
            };

            _context.Events.Add(eventEntity);

            await _context.SaveChangesAsync();

            // El evento nace como Draft, por lo tanto NO podemos
            // utilizar GetPublicByIdAsync.
            return await GetMineByIdAsync(
                eventEntity.Id,
                organizerId);
        }

        // =====================================================
        // UPDATE
        // =====================================================

        public async Task<ServiceResult<EventDetailDto>> UpdateAsync(
            int id,
            UpdateEventDto dto,
            int organizerId)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.OrganizerId == organizerId);

            if (eventEntity is null)
            {
                return ServiceResult<EventDetailDto>
                    .Missing(
                        "El evento no existe o no pertenece al usuario.");
            }

            if (eventEntity.Status == EventStatus.Cancelled)
            {
                return ServiceResult<EventDetailDto>
                    .Failure(
                        "No se puede editar un evento cancelado.");
            }

            if (eventEntity.Status == EventStatus.Finished)
            {
                return ServiceResult<EventDetailDto>
                    .Failure(
                        "No se puede editar un evento finalizado.");
            }

            var categoryExists = await _context.EventCategories
                .AnyAsync(c =>
                    c.Id == dto.EventCategoryId &&
                    c.IsActive);

            if (!categoryExists)
            {
                return ServiceResult<EventDetailDto>
                    .Failure(
                        "La categoría seleccionada no existe o está inactiva.");
            }

            var newEventDateTime =
                dto.Date.Date + dto.StartTime;

            if (eventEntity.Status == EventStatus.Published &&
                newEventDateTime <= DateTime.Now)
            {
                return ServiceResult<EventDetailDto>
                    .Failure(
                        "Un evento publicado debe tener una fecha y hora futura.");
            }

            eventEntity.Title = dto.Title.Trim();
            eventEntity.Description = dto.Description.Trim();
            eventEntity.Date = dto.Date.Date;
            eventEntity.StartTime = dto.StartTime;
            eventEntity.Location = dto.Location.Trim();
            eventEntity.Capacity = dto.Capacity;
            eventEntity.ImageUrl = dto.ImageUrl;
            eventEntity.EventCategoryId = dto.EventCategoryId;

            await _context.SaveChangesAsync();

            var updatedEvent =
                await GetMineByIdAsync(
                    id,
                    organizerId);

            return ServiceResult<EventDetailDto>
                .Ok(updatedEvent!);
        }

        // =====================================================
        // PUBLISH
        // =====================================================

        public async Task<ServiceResult<EventDetailDto>> PublishAsync(
            int id,
            int organizerId)
        {
            var eventEntity = await _context.Events
                .Include(e => e.EventCategory)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.OrganizerId == organizerId);

            if (eventEntity is null)
            {
                return ServiceResult<EventDetailDto>
                    .Missing(
                        "El evento no existe o no pertenece al usuario.");
            }

            if (eventEntity.Status != EventStatus.Draft)
            {
                return ServiceResult<EventDetailDto>
                    .Failure(
                        "Solo los eventos en borrador pueden publicarse.");
            }

            if (!eventEntity.EventCategory.IsActive)
            {
                return ServiceResult<EventDetailDto>
                    .Failure(
                        "La categoría del evento está inactiva.");
            }

            var eventDateTime =
                eventEntity.Date.Date +
                eventEntity.StartTime;

            if (eventDateTime <= DateTime.Now)
            {
                return ServiceResult<EventDetailDto>
                    .Failure(
                        "No se puede publicar un evento cuya fecha y hora ya pasaron.");
            }

            eventEntity.Status =
                EventStatus.Published;

            await _context.SaveChangesAsync();

            var publishedEvent =
                await GetMineByIdAsync(
                    id,
                    organizerId);

            return ServiceResult<EventDetailDto>
                .Ok(publishedEvent!);
        }

        // =====================================================
        // CANCEL
        // =====================================================

        public async Task<ServiceResult<EventDetailDto>> CancelAsync(
            int id,
            int organizerId)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.OrganizerId == organizerId);

            if (eventEntity is null)
            {
                return ServiceResult<EventDetailDto>
                    .Missing(
                        "El evento no existe o no pertenece al usuario.");
            }

            if (eventEntity.Status != EventStatus.Published)
            {
                return ServiceResult<EventDetailDto>
                    .Failure(
                        "Solo los eventos publicados pueden cancelarse.");
            }

            eventEntity.Status =
                EventStatus.Cancelled;

            await _context.SaveChangesAsync();

            var cancelledEvent =
                await GetMineByIdAsync(
                    id,
                    organizerId);

            return ServiceResult<EventDetailDto>
                .Ok(cancelledEvent!);
        }
    }
}