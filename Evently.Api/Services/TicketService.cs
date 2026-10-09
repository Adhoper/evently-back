using Evently.Api.Data;
using Evently.Api.DTOs.Tickets;
using Evently.Api.Models;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Evently.Api.Services.Results;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Evently.Api.Services
{
    public class TicketService : ITicketService
    {
        private readonly EventlyDbContext _context;

        public TicketService(
            EventlyDbContext context)
        {
            _context = context;
        }

        public async Task<List<TicketDto>> GetMineAsync(
            int userId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.ReservedAt)
                .Select(t => new TicketDto
                {
                    Id = t.Id,
                    Code = t.Code,
                    Status = t.Status.ToString(),
                    ReservedAt = t.ReservedAt,
                    CheckedInAt = t.CheckedInAt,

                    EventId = t.EventId,
                    EventTitle = t.Event.Title,
                    EventDate = t.Event.Date,
                    EventStartTime = t.Event.StartTime,
                    EventLocation = t.Event.Location,
                    EventImageUrl = t.Event.ImageUrl,

                    CategoryName =
                        t.Event.EventCategory.Name
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<TicketDto>>
            GetMineByIdAsync(
                int ticketId,
                int userId)
        {
            var ticket = await _context.Tickets
                .AsNoTracking()
                .Where(t =>
                    t.Id == ticketId &&
                    t.UserId == userId)
                .Select(t => new TicketDto
                {
                    Id = t.Id,
                    Code = t.Code,
                    Status = t.Status.ToString(),
                    ReservedAt = t.ReservedAt,
                    CheckedInAt = t.CheckedInAt,

                    EventId = t.EventId,
                    EventTitle = t.Event.Title,
                    EventDate = t.Event.Date,
                    EventStartTime = t.Event.StartTime,
                    EventLocation = t.Event.Location,
                    EventImageUrl = t.Event.ImageUrl,

                    CategoryName =
                        t.Event.EventCategory.Name
                })
                .FirstOrDefaultAsync();

            if (ticket is null)
            {
                return ServiceResult<TicketDto>
                    .Missing(
                        "La entrada no existe o no pertenece al usuario.");
            }

            return ServiceResult<TicketDto>
                .Ok(ticket);
        }

        public async Task<ServiceResult<TicketDto>> ReserveAsync(
            int eventId,
            int userId)
        {
            
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var userExists =
                await _context.Users.AnyAsync(u =>
                    u.Id == userId &&
                    u.IsActive);

            if (!userExists)
            {
                return ServiceResult<TicketDto>
                    .Failure(
                        "La cuenta no se encuentra disponible.");
            }

            var eventEntity =
                await _context.Events
                    .FirstOrDefaultAsync(e =>
                        e.Id == eventId);

            if (eventEntity is null)
            {
                return ServiceResult<TicketDto>
                    .Missing(
                        "El evento no existe.");
            }

            if (eventEntity.Status !=
                EventStatus.Published)
            {
                return ServiceResult<TicketDto>
                    .Failure(
                        "Este evento no está disponible para reservas.");
            }

            var eventDateTime =
                eventEntity.Date.Date +
                eventEntity.StartTime;

            if (eventDateTime <= DateTime.Now)
            {
                return ServiceResult<TicketDto>
                    .Failure(
                        "No se pueden reservar entradas para un evento que ya inició.");
            }

            var existingTicket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.EventId == eventId &&
                        t.UserId == userId);

            if (existingTicket is not null &&
                existingTicket.Status !=
                    TicketStatus.Cancelled)
            {
                return ServiceResult<TicketDto>
                    .Failure(
                        "Ya tienes una entrada para este evento.");
            }

            var occupiedSpots =
                await _context.Tickets
                    .CountAsync(t =>
                        t.EventId == eventId &&
                        t.Status != TicketStatus.Cancelled);

            if (occupiedSpots >=
                eventEntity.Capacity)
            {
                return ServiceResult<TicketDto>
                    .Failure(
                        "No quedan entradas disponibles para este evento.");
            }

            if (existingTicket is not null)
            {
                

                existingTicket.Status =
                    TicketStatus.Reserved;

                existingTicket.Code =
                    GenerateTicketCode();

                existingTicket.ReservedAt =
                    DateTime.UtcNow;

                existingTicket.CheckedInAt =
                    null;
            }
            else
            {
                var ticket = new Ticket
                {
                    Code =
                        GenerateTicketCode(),

                    Status =
                        TicketStatus.Reserved,

                    ReservedAt =
                        DateTime.UtcNow,

                    EventId =
                        eventId,

                    UserId =
                        userId
                };

                _context.Tickets.Add(ticket);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var savedTicket =
                await _context.Tickets
                    .AsNoTracking()
                    .Where(t =>
                        t.EventId == eventId &&
                        t.UserId == userId)
                    .Select(t => new TicketDto
                    {
                        Id = t.Id,
                        Code = t.Code,
                        Status = t.Status.ToString(),
                        ReservedAt = t.ReservedAt,
                        CheckedInAt = t.CheckedInAt,

                        EventId = t.EventId,
                        EventTitle = t.Event.Title,
                        EventDate = t.Event.Date,
                        EventStartTime =
                            t.Event.StartTime,
                        EventLocation =
                            t.Event.Location,
                        EventImageUrl =
                            t.Event.ImageUrl,

                        CategoryName =
                            t.Event.EventCategory.Name
                    })
                    .FirstAsync();

            return ServiceResult<TicketDto>
                .Ok(savedTicket);
        }

        public async Task<ServiceResult<TicketDto>> CancelAsync(
            int ticketId,
            int userId)
        {
            var ticket =
                await _context.Tickets
                    .Include(t => t.Event)
                    .FirstOrDefaultAsync(t =>
                        t.Id == ticketId &&
                        t.UserId == userId);

            if (ticket is null)
            {
                return ServiceResult<TicketDto>
                    .Missing(
                        "La entrada no existe o no pertenece al usuario.");
            }

            if (ticket.Status ==
                TicketStatus.Cancelled)
            {
                return ServiceResult<TicketDto>
                    .Failure(
                        "La entrada ya se encuentra cancelada.");
            }

            if (ticket.Status ==
                TicketStatus.CheckedIn)
            {
                return ServiceResult<TicketDto>
                    .Failure(
                        "Una entrada utilizada no puede cancelarse.");
            }

            var eventDateTime =
                ticket.Event.Date.Date +
                ticket.Event.StartTime;

            if (eventDateTime <= DateTime.Now)
            {
                return ServiceResult<TicketDto>
                    .Failure(
                        "No se puede cancelar una entrada después de iniciado el evento.");
            }

            ticket.Status =
                TicketStatus.Cancelled;

            await _context.SaveChangesAsync();

            return await GetMineByIdAsync(
                ticket.Id,
                userId);
        }

        public async Task<ServiceResult<CheckInResultDto>>
            CheckInAsync(
                string code,
                int organizerId)
        {
            var normalizedCode =
                code.Trim().ToUpperInvariant();

            var ticket =
                await _context.Tickets
                    .Include(t => t.Event)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t =>
                        t.Code ==
                        normalizedCode);

            if (ticket is null)
            {
                return ServiceResult<CheckInResultDto>
                    .Missing(
                        "La entrada no existe.");
            }

            
            if (ticket.Event.OrganizerId !=
                organizerId)
            {
                return ServiceResult<CheckInResultDto>
                    .Missing(
                        "La entrada no pertenece a uno de tus eventos.");
            }

            if (ticket.Event.Status !=
                EventStatus.Published)
            {
                return ServiceResult<CheckInResultDto>
                    .Failure(
                        "El evento no está disponible para check-in.");
            }

            if (ticket.Status ==
                TicketStatus.Cancelled)
            {
                return ServiceResult<CheckInResultDto>
                    .Failure(
                        "Esta entrada fue cancelada.");
            }

            if (ticket.Status ==
                TicketStatus.CheckedIn)
            {
                return ServiceResult<CheckInResultDto>
                    .Failure(
                        "Esta entrada ya fue utilizada.");
            }

            ticket.Status =
                TicketStatus.CheckedIn;

            ticket.CheckedInAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response =
                new CheckInResultDto
                {
                    TicketId =
                        ticket.Id,

                    Code =
                        ticket.Code,

                    Status =
                        ticket.Status.ToString(),

                    CheckedInAt =
                        ticket.CheckedInAt.Value,

                    EventId =
                        ticket.EventId,

                    EventTitle =
                        ticket.Event.Title,

                    AttendeeName =
                        $"{ticket.User.FirstName} {ticket.User.LastName}",

                    AttendeeEmail =
                        ticket.User.Email
                };

            return ServiceResult<CheckInResultDto>
                .Ok(response);
        }

        private static string GenerateTicketCode()
        {
            return Guid.NewGuid()
                .ToString("N")
                .ToUpperInvariant();
        }
    }
}