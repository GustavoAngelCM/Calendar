using Microsoft.AspNetCore.Mvc;
using Calendar.Data;
using Calendar.Models;
using Calendar.Services;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Calendar.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Calendar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (exists)
                return BadRequest("Email ya registrado");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized("Credenciales inválidas");

            var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

            if (!valid)
                return Unauthorized("Credenciales inválidas");

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token
            });
        }


        [HttpGet("verify")]
        public async Task<IActionResult> Verify()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Token no proporcionado");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {

                var principal = _jwtService.VerifyToken(token);

                if (principal == null)
                    return Unauthorized("Token inválido o expirado.");

                var userIdClaim = principal.Claims.FirstOrDefault()?.Value;

                if (userIdClaim == null)
                    return Unauthorized("Token malformado.");

                var user = await _context.Users
                    .Select(u => new { u.Id, u.Name, u.Email })
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userIdClaim);

                if (user == null)
                    return NotFound("Usuario no encontrado.");

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized("Error al validar token");
            }
        }

    }
}