using System.Security.Claims;
using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstrhoApp.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly UserService _userService;

        public AuthController(AstrhoAppDbContext context, JwtService jwtService, UserService userService)
        {
            _context = context;
            _jwtService = jwtService;
            _userService = userService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                    .ThenInclude(r => r.RolPermisos)
                        .ThenInclude(rp => rp.Permiso)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null || !usuario.Estado ||
                !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Contrasena))
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Credenciales inválidas"
                });
            }

            var permisos = usuario.Rol?.RolPermisos?
                .Where(rp => rp.Permiso != null)
                .Select(rp => rp.Permiso!.Nombre)
                .Distinct()
                .ToList() ?? new List<string>();

            var token = _jwtService.GenerarTokenLogin(usuario, permisos);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Autenticado",
                Token = token,
                UsuarioId = usuario.UsuarioId,
                Email = usuario.Email,
                Rol = usuario.Rol?.Nombre,
                Permisos = permisos,
                MustChangePassword = usuario.MustChangePassword
            });
        }

        [HttpPost("create-temp-user")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateTempUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var tempPassword = await _userService.CreateUserWithTemporaryPasswordAsync(dto);
                return Ok(new 
                { 
                    success = true, 
                    message = "Usuario creado y contraseña enviada por correo.",
                    tempPassword // Se muestra para facilitar pruebas/admin
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno al crear usuario." });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Unauthorized();

            var usuarioId = int.Parse(claim.Value);

            var ok = await _userService.ChangePasswordAsync(
                usuarioId,
                dto.CurrentPassword,
                dto.NewPassword
            );

            if (!ok)
                return BadRequest(new { success = false, message = "Cambio fallido" });

            return Ok(new { success = true });
        }
    }
}