using Calendar.Data;
using Calendar.Helpers;
using Calendar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var userLoggedId = User.GetUserId();

            var users = await _context.Users
                .Where(u => u.Active && u.DeletedAt == null && u.Id != userLoggedId )
                .OrderBy(u => u.Name)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
