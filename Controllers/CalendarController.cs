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
        [HttpGet("calendar/{typeStatusCalendar}")]
        public async Task<IActionResult> GetCalendar(ParticipationStatus typeStatusCalendar)
        {
            var userLoggedId = User.GetUserId();

            var events = await _context.Events
                .Where(e => e.Active && e.DeletedAt == null)
                .Where(e => e.Participations.Any(p =>
                    p.UserId == userLoggedId &&
                    p.Status == typeStatusCalendar &&
                    p.Active &&
                    p.DeletedAt == null
                ))
                .OrderBy(e => e.DateEvent)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.DateEvent,

                    Detail = e.Participations
                        .Where(p =>
                            p.UserId == userLoggedId &&
                            p.Active &&
                            p.DeletedAt == null
                        )
                        .Select(p => new
                        {
                            p.IsCreator,
                            p.Status,
                            p.RespondedAt,
                            User = new
                            {
                                p.User.Id,
                                p.User.Name,
                                p.User.Email
                            },
                            InvitedBy = p.InvitedByUserId != null
                                ? _context.Users
                                    .Where(u => u.Id == p.InvitedByUserId)
                                    .Select(u => new
                                    {
                                        u.Id,
                                        u.Name
                                    })
                                    .FirstOrDefault()
                                : null
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(events);
        }
    }
}
