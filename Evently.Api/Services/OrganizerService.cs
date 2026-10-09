using Evently.Api.Data;
using Evently.Api.DTOs.Organizer;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Evently.Api.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Services
{
    public class OrganizerService : IOrganizerService
    {
        private readonly EventlyDbContext _context;

        public OrganizerService(
            EventlyDbContext context)
        {
            _context = context;
        }

        public async Task<OrganizerDashboardDto> GetDashboardAsync(
            int organizerId)
        {

            var events = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    e.OrganizerId == organizerId)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Status,
                    e.Capacity
                })
                .ToListAsync();

            if (events.Count == 0)
            {
                return new OrganizerDashboardDto();
            }
            var activeEvents = events
                .Where(e =>
                    e.Status == EventStatus.Published ||
                    e.Status == EventStatus.Finished)
                .ToList();

            var activeEventIds = activeEvents
                .Select(e => e.Id)
                .ToList();

            var tickets = await _context.Tickets
                .AsNoTracking()
                .Where(t =>
                    activeEventIds.Contains(t.EventId) &&
                    t.Status != TicketStatus.Cancelled)
                .Select(t => new
                {
                    t.EventId,
                    t.Status
                })
                .ToListAsync();

            var totalReservations =
                tickets.Count;

            var totalCheckIns =
                tickets.Count(t =>
                    t.Status == TicketStatus.CheckedIn);

            var totalCapacity =
                activeEvents.Sum(e =>
                    e.Capacity);

            var attendanceRate =
                totalReservations > 0
                    ? Math.Round(
                        (double)totalCheckIns /
                        totalReservations *
                        100,
                        2)
                    : 0;

            var occupancyRate =
                totalCapacity > 0
                    ? Math.Round(
                        (double)totalReservations /
                        totalCapacity *
                        100,
                        2)
                    : 0;

            TopEventDto? topEvent =
                null;

            if (activeEvents.Count > 0)
            {
                var eventStats =
                    activeEvents
                        .Select(e =>
                        {
                            var reservations =
                                tickets.Count(t =>
                                    t.EventId == e.Id);

                            var checkIns =
                                tickets.Count(t =>
                                    t.EventId == e.Id &&
                                    t.Status ==
                                        TicketStatus.CheckedIn);

                            var eventOccupancy =
                                e.Capacity > 0
                                    ? Math.Round(
                                        (double)reservations /
                                        e.Capacity *
                                        100,
                                        2)
                                    : 0;

                            return new TopEventDto
                            {
                                Id = e.Id,

                                Title =
                                    e.Title,

                                Reservations =
                                    reservations,

                                CheckIns =
                                    checkIns,

                                Capacity =
                                    e.Capacity,

                                OccupancyRate =
                                    eventOccupancy
                            };
                        })
                        .OrderByDescending(e =>
                            e.Reservations)
                        .ThenByDescending(e =>
                            e.OccupancyRate)
                        .FirstOrDefault();

                if (
                    eventStats is not null &&
                    eventStats.Reservations > 0)
                {
                    topEvent =
                        eventStats;
                }
            }

            return new OrganizerDashboardDto
            {
                TotalEvents =
                    events.Count,

                PublishedEvents =
                    events.Count(e =>
                        e.Status ==
                        EventStatus.Published),

                DraftEvents =
                    events.Count(e =>
                        e.Status ==
                        EventStatus.Draft),

                CancelledEvents =
                    events.Count(e =>
                        e.Status ==
                        EventStatus.Cancelled),

                FinishedEvents =
                    events.Count(e =>
                        e.Status ==
                        EventStatus.Finished),

                TotalReservations =
                    totalReservations,

                TotalCheckIns =
                    totalCheckIns,

                TotalCapacity =
                    totalCapacity,

                AttendanceRate =
                    attendanceRate,

                OccupancyRate =
                    occupancyRate,

                TopEvent =
                    topEvent
            };
        }

        public async Task<ServiceResult<EventAttendeesDto>>
            GetEventAttendeesAsync(
                int eventId,
                int organizerId)
        {

            var eventEntity =
                await _context.Events
                    .AsNoTracking()
                    .Where(e =>
                        e.Id == eventId &&
                        e.OrganizerId == organizerId)
                    .Select(e => new
                    {
                        e.Id,
                        e.Title,
                        e.Status,
                        e.Date,
                        e.StartTime,
                        e.Location,
                        e.Capacity
                    })
                    .FirstOrDefaultAsync();

            if (eventEntity is null)
            {
                return ServiceResult<EventAttendeesDto>
                    .Missing(
                        "El evento no existe o no pertenece al organizador.");
            }

            var attendees =
                await _context.Tickets
                    .AsNoTracking()
                    .Where(t =>
                        t.EventId == eventId)
                    .OrderByDescending(t =>
                        t.ReservedAt)
                    .Select(t =>
                        new AttendeeDto
                        {
                            TicketId =
                                t.Id,

                            UserId =
                                t.UserId,

                            FullName =
                                t.User.FirstName +
                                " " +
                                t.User.LastName,

                            Email =
                                t.User.Email,

                            Status =
                                t.Status.ToString(),

                            ReservedAt =
                                t.ReservedAt,

                            CheckedInAt =
                                t.CheckedInAt
                        })
                    .ToListAsync();

            var reservedCount =
                attendees.Count(a =>
                    a.Status !=
                    TicketStatus.Cancelled.ToString());

            var checkedInCount =
                attendees.Count(a =>
                    a.Status ==
                    TicketStatus.CheckedIn.ToString());

            var cancelledCount =
                attendees.Count(a =>
                    a.Status ==
                    TicketStatus.Cancelled.ToString());

            var availableSpots =
                Math.Max(
                    0,
                    eventEntity.Capacity -
                    reservedCount);

            var occupancyRate =
                eventEntity.Capacity > 0
                    ? Math.Round(
                        (double)reservedCount /
                        eventEntity.Capacity *
                        100,
                        2)
                    : 0;

            var attendanceRate =
                reservedCount > 0
                    ? Math.Round(
                        (double)checkedInCount /
                        reservedCount *
                        100,
                        2)
                    : 0;

            var response =
                new EventAttendeesDto
                {
                    EventId =
                        eventEntity.Id,

                    Title =
                        eventEntity.Title,

                    Status =
                        eventEntity.Status.ToString(),

                    Date =
                        eventEntity.Date,

                    StartTime =
                        eventEntity.StartTime,

                    Location =
                        eventEntity.Location,

                    Capacity =
                        eventEntity.Capacity,

                    ReservedCount =
                        reservedCount,

                    CheckedInCount =
                        checkedInCount,

                    CancelledCount =
                        cancelledCount,

                    AvailableSpots =
                        availableSpots,

                    OccupancyRate =
                        occupancyRate,

                    AttendanceRate =
                        attendanceRate,

                    Attendees =
                        attendees
                };

            return ServiceResult<EventAttendeesDto>
                .Ok(response);
        }
    }
}