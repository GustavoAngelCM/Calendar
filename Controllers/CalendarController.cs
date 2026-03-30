using Calendar.Data;
using Calendar.DTOs;
using Calendar.Helpers;
using Calendar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Calendar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CalendarController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateResponseEvent(ParticipationResponseEventDto dto)
        {
            try
            {
                var userLoggedId = User.GetUserId();

                var participation = await _context.Participations
                    .FirstOrDefaultAsync(p =>
                        p.UserId == userLoggedId &&
                        p.EventId == dto.Id &&
                        p.Active &&
                        p.DeletedAt == null
                    );

                if (participation == null)
                {
                    return BadRequest("La invitacion a este evento no existe.");
                }

                participation.Status = dto.ParticipationStatus;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Respuesta al evento guardado." });
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Authorize]
        [HttpGet("calendar/{typeStatusCalendar}")]
        public async Task<IActionResult> GetCalendar(ParticipationStatusDto typeStatusCalendar)
        {
            var userLoggedId = User.GetUserId();
            var status = (typeStatusCalendar == ParticipationStatusDto.Pending ? ParticipationStatus.Pending :
            (
                typeStatusCalendar == ParticipationStatusDto.Accepted ? ParticipationStatus.Accepted :
                (
                    typeStatusCalendar == ParticipationStatusDto.Rejected ? ParticipationStatus.Rejected : ParticipationStatus.Pending
                )
            ));

            var eventsDb = await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Participations)
                        .ThenInclude(p => p.User)
                    .Where(e => e.Active && e.DeletedAt == null)
                    .Where(e => e.Participations.Any(p =>
                        (
                            typeStatusCalendar == ParticipationStatusDto.Other
                                ? p.UserId != userLoggedId && e.TypeEvent != TypeEvent.Exclusive
                                : p.UserId == userLoggedId
                        ) &&
                        p.Status == status &&
                        p.Active &&
                        p.DeletedAt == null
                    ))
                    .OrderBy(e => e.DateEvent)
                    .ToListAsync();

            var events = eventsDb.Select(e =>
            {
                var participation = e.Participations
                    .Where(pa =>
                        typeStatusCalendar == ParticipationStatusDto.Pending
                            ? (pa.UserId == userLoggedId && !pa.IsCreator)
                            : (
                                typeStatusCalendar == ParticipationStatusDto.Accepted 
                                ? (pa.UserId == userLoggedId)
                                : (
                                    typeStatusCalendar == ParticipationStatusDto.Rejected
                                    ? (pa.UserId == userLoggedId && !pa.IsCreator) :
                                    (pa.UserId != userLoggedId && !pa.IsCreator && pa.Status != ParticipationStatus.Rejected)
                                )
                            )
                    )
                    .FirstOrDefault(p =>
                        typeStatusCalendar == ParticipationStatusDto.Other
                            ? (
                                p.Active &&
                                p.DeletedAt == null
                            ) : (
                                p.UserId == userLoggedId &&
                                p.Active &&
                                p.DeletedAt == null
                            )
                    );

                if (participation == null) return null;

                return new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.DateEvent,
                    e.DateEndEvent,
                    e.Location,
                    e.TypeEvent,
                    e.Category,

                    Detail = new
                    {
                        participation.IsCreator,
                        participation.Status,
                        participation.RespondedAt,

                        GuestUsers = participation.IsCreator
                            ? e.Participations
                                .Where(p => p.Active && p.DeletedAt == null && p.UserId != userLoggedId)
                                .Select(p => new
                                {
                                    p.User.Id,
                                    p.User.Name
                                })
                                .ToList()
                            : null,

                        User = new
                        {
                            participation.User.Id,
                            participation.User.Name,
                            participation.User.Email
                        },

                        InvitedBy = participation.InvitedByUserId != null && typeStatusCalendar != ParticipationStatusDto.Other
                            ? e.Participations
                                .Where(p => p.UserId == participation.InvitedByUserId)
                                .Select(p => new
                                {
                                    p.User.Id,
                                    p.User.Name
                                })
                                .FirstOrDefault()
                            : null
                    }
                };
            })
            .Where(e => e != null)
            .ToList();

            return Ok(events);
        }
    }
}
