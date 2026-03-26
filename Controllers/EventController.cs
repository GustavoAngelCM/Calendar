using Calendar.Data;
using Calendar.DTOs;
using Calendar.Helpers;
using Calendar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Calendar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController: ControllerBase
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateEventDto dto)
        {
            var userIdLogged = User.GetUserId();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var exists = await _context.Events
                    .AnyAsync(u => u.Name == dto.Name.ToUpper());

                if (exists && dto.ForcedNametag)
                    return BadRequest("Evento ya registrado.");

                var superPositionExclusive = await _context.Events
                    .AnyAsync(e =>
                        e.TypeEvent == TypeEvent.Exclusive &&
                        e.DateEvent == dto.DateEvent
                    );

                if (superPositionExclusive)
                    return BadRequest("El evento exclusivo no puede superponerse.");

                var newEvent = new Event
                {
                    Name = dto.Name.ToUpper(),
                    Description = dto.Description,
                    DateEvent = dto.DateEvent,
                    Location = dto.Location,
                    BgColor = dto.BgColor,
                    TypeEvent = dto.TypeEvent
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                var participations = new List<Participation>
                {
                    new Participation
                    {
                        EventId = newEvent.Id,
                        UserId = userIdLogged,
                        IsCreator = true,
                        Status = ParticipationStatus.Accepted
                    }
                };

                var uniqueIds = dto.ParticipantsIds
                    .Where(id => id != userIdLogged)
                    .Distinct()
                    .ToList();

                participations.AddRange(uniqueIds.Select(id => new Participation
                {
                    EventId = newEvent.Id,
                    UserId = id,
                    IsCreator = false,
                    Status = ParticipationStatus.Pending
                }));

                _context.Participations.AddRange(participations);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(newEvent);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateEventDto dto)
        {
            var userIdLogged = User.GetUserId();

            var ev = await _context.Events
                .Include(e => e.Participations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound("Evento no encontrado.");

            var isCreator = ev.Participations
                .Any(p => p.UserId == userIdLogged && p.IsCreator);

            if (!isCreator)
                return Forbid("No tienes permisos para editar este evento.");

            var superPositionExclusive = await _context.Events
                .AnyAsync(e =>
                    e.TypeEvent == TypeEvent.Exclusive &&
                    e.DateEvent == dto.DateEvent &&
                    e.Id != id
                );

            if (superPositionExclusive)
                return BadRequest("El evento exclusivo no puede superponerse.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                ev.Name = dto.Name.ToUpper();
                ev.Description = dto.Description;
                ev.Location = dto.Location;
                ev.DateEvent = dto.DateEvent;
                ev.BgColor = dto.BgColor;
                ev.TypeEvent = dto.TypeEvent;
                ev.UpdatedAt = DateTime.UtcNow;

                var currentParticipants = ev.Participations
                    .Where(p => !p.IsCreator)
                    .ToList();

                var newIds = dto.ParticipantsIds
                    .Where(id => id != userIdLogged)
                    .Distinct()
                    .ToList();

                var toRemove = currentParticipants
                    .Where(p => !newIds.Contains(p.UserId))
                    .ToList();

                var toAdd = newIds
                    .Where(id => !currentParticipants.Any(p => p.UserId == id))
                    .ToList();

                _context.Participations.RemoveRange(toRemove);

                _context.Participations.AddRange(toAdd.Select(id => new Participation
                {
                    EventId = ev.Id,
                    UserId = id,
                    Status = ParticipationStatus.Pending
                }));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ev);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdLogged = User.GetUserId();

            var ev = await _context.Events
                .Include(e => e.Participations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound("Evento no encontrado.");

            var isCreator = ev.Participations
                .Any(p => p.UserId == userIdLogged && p.IsCreator);

            if (!isCreator)
                return Forbid("No tienes permisos para eliminar este evento.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                ev.DeletedAt = DateTime.UtcNow;
                ev.Active = false;

                _context.Participations.RemoveRange(ev.Participations);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ev);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
