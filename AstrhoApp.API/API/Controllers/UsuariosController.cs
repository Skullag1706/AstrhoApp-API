using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using AstrhoApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AstrhoApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public UsuariosController(
            AstrhoAppDbContext context,
            JwtService jwtService,
            EmailService emailService,
            IConfiguration config)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
            _config = config;
        }

        // ============================================================
        // MÉTODO AUXILIAR
        // ============================================================
        private string GenerarCodigoRecuperacion()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        // ============================================================
        // LISTAR USUARIOS
        // ============================================================
        [HttpGet]
        [Authorize(Policy = "perm:Usuarios")]
        public async Task<ActionResult> ListarUsuarios([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Usuarios
                    .Include(u => u.Rol)
                    .AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(u => 
                        u.Email.ToLower().Contains(busqueda) || 
                        (u.Rol != null && u.Rol.Nombre.ToLower().Contains(busqueda))
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var usuarios = await query
                    .OrderBy(u => u.Email)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(u => new UsuarioListDto
                    {
                        UsuarioId = u.UsuarioId,
                        Email = u.Email,
                        Estado = u.Estado,
                        RolNombre = u.Rol.Nombre
                    })
                    .ToListAsync();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = usuarios 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al listar usuarios: {ex.Message}" });
            }
        }

        // ============================================================
        // BUSCAR USUARIO POR ID
        // ============================================================
        [HttpGet("{id}")]
        [Authorize(Policy = "perm:Usuarios")]
        public async Task<ActionResult<UsuarioResponseDto>> BuscarUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(new UsuarioResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                Email = usuario.Email,
                Estado = usuario.Estado,
                Rol = new RolDto
                {
                    RolId = usuario.Rol.RolId,
                    Nombre = usuario.Rol.Nombre,
                    Descripcion = usuario.Rol.Descripcion
                }
            });
        }

        // ============================================================
        // REGISTRAR USUARIO (PÚBLICO / PRUEBAS)
        // ============================================================
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RegistrarUsuario([FromBody] CrearUsuarioDto dto)
        {
            if (dto.Contrasena != dto.ConfirmarContrasena)
                return BadRequest("Las contraseñas no coinciden");

            if (dto.Contrasena.Length < 6)
                return BadRequest("La contraseña debe tener al menos 6 caracteres");

            var usuario = new Usuario
            {
                RolId = dto.RolId,
                Email = dto.Email,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena),
                Estado = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // ============================================================
        // ACTUALIZAR USUARIO
        // ============================================================
        [HttpPut("{id}")]
        [Authorize(policy: "perm:Usuarios")]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            usuario.Email = dto.Email;
            usuario.RolId = dto.RolId;
            usuario.Estado = dto.Estado;

            // Cambiar contraseña SOLO si se envía
            if (!string.IsNullOrWhiteSpace(dto.Contrasena))
            {
                if (dto.Contrasena != dto.ConfirmarContrasena)
                    return BadRequest("Las contraseñas no coinciden");

                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        [Authorize(policy: "perm:Usuarios")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Clientes)
                    .Include(u => u.Empleados)
                    .FirstOrDefaultAsync(u => u.UsuarioId == id);

                if (usuario == null)
                    return NotFound("Usuario no encontrado");

                // Verificar si tiene clientes asociados y si estos se pueden eliminar
                foreach (var cliente in usuario.Clientes)
                {
                    var tieneCitas = await _context.Agenda.AnyAsync(a => a.DocumentoCliente == cliente.DocumentoCliente);
                    var tieneVentas = await _context.Venta.AnyAsync(v => v.DocumentoCliente == cliente.DocumentoCliente);

                    if (tieneCitas || tieneVentas)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"No se puede eliminar el usuario porque su cliente asociado ({cliente.Nombre}) tiene registros (citas o ventas)"
                        });
                    }
                    _context.Clientes.Remove(cliente);
                }

                // Verificar si tiene empleados asociados y si estos se pueden eliminar
                foreach (var empleado in usuario.Empleados)
                {
                    var tieneCitas = await _context.Agenda.AnyAsync(a => a.DocumentoEmpleado == empleado.DocumentoEmpleado);
                    var tieneEntregas = await _context.Entregainsumos.AnyAsync(ei => ei.DocumentoEmpleado == empleado.DocumentoEmpleado);
                    var tieneHorarios = await _context.HorarioEmpleados.AnyAsync(he => he.DocumentoEmpleado == empleado.DocumentoEmpleado);

                    if (tieneCitas || tieneEntregas || tieneHorarios)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"No se puede eliminar el usuario porque su empleado asociado ({empleado.Nombre}) tiene registros (citas, entregas o horarios)"
                        });
                    }
                    _context.Empleados.Remove(empleado);
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Usuario y sus registros asociados eliminados exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar usuario: {ex.Message}"
                });
            }
        }

        // ============================================================
        // RECUPERACIÓN DE CONTRASEÑA - ENVIAR CÓDIGO (PÚBLICO)
        // ============================================================
        [AllowAnonymous]
        [HttpPost("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword([FromBody] SolicitarRecuperacionDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
                return Ok(); // seguridad

            var codigo = GenerarCodigoRecuperacion();
            var token = _jwtService.GenerarTokenRecuperacion(usuario, codigo);

            await _emailService.EnviarCodigoRecuperacion(usuario.Email, codigo);

            return Ok(new { success = true, token });
        }

        // ============================================================
        // RECUPERACIÓN DE CONTRASEÑA - VALIDAR CÓDIGO (PÚBLICO)
        // ============================================================

        [AllowAnonymous]
        [HttpPost("validar-codigo-recuperacion")]
        public async Task<IActionResult> ValidarCodigoRecuperacion(
            [FromBody] ValidarCodigoDto dto)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = _config["JwtSettings:Issuer"],
                    ValidAudiences = new[]
                    {
                        _config["JwtSettings:Audience"],
                        _config["JwtSettings:RecoveryAudience"]
                    },

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["JwtSettings:RecoverySecretKey"]!)
                    ),

                    ClockSkew = TimeSpan.Zero
                }, out _);


                // 2️⃣ Validar propósito
                if (principal.Claims.First(c => c.Type == "purpose").Value != "password_reset")
                    return Unauthorized("Token inválido");

                // 3️⃣ Obtener usuario
                var userId = int.Parse(
                    principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value
                );

                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario == null)
                    return NotFound("Usuario no encontrado");

                // 5️⃣ Validar código
                var hash = principal.Claims.First(c => c.Type == "code_hash").Value;

                if (!BCrypt.Net.BCrypt.Verify(dto.Codigo, hash))
                    return Unauthorized("Código incorrecto");

                // 6️⃣ Generar Token B (reset final)
                var resetToken = _jwtService.GenerarTokenResetFinal(usuario);

                return Ok(new
                {
                    valid = true,
                    resetToken
                });
            }
            catch
            {
                return Unauthorized("Código expirado o inválido");
            }
        }


        // ============================================================
        // RECUPERACIÓN DE CONTRASEÑA - RESET (PÚBLICO)
        // ============================================================
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NuevaContrasena != dto.ConfirmarContrasena)
                return BadRequest("Las contraseñas no coinciden");

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(dto.ResetToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = _config["JwtSettings:Issuer"],
                    ValidAudience = _config["JwtSettings:ResetFinalAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["JwtSettings:ResetFinalSecretKey"]!)
                    ),

                    ClockSkew = TimeSpan.Zero
                }, out _);

                if (principal.Claims.First(c => c.Type == "purpose").Value != "password_reset_final")
                    return Unauthorized();

                var userId = int.Parse(
                    principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value
                );

                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario == null)
                    return NotFound();

                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(dto.NuevaContrasena);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch
            {
                return Unauthorized("Token inválido o expirado");
            }
        }
    }
}
