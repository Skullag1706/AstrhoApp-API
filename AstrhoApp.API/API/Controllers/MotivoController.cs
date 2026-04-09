using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using AstrhoApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MotivoController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;
        private readonly EmailService _emailService;

        public MotivoController(AstrhoAppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ===============================
        // 🧠 Helper: obtener usuarioId desde JWT
        // ===============================
        private int? GetUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return null;

            return int.Parse(claim.Value);
        }

        // ===============================
        // 📝 POST: Crear motivo
        // ===============================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CrearMotivo([FromBody] MotivoCreateDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null)
                return Unauthorized("Token inválido");

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);

            if (empleado == null)
                return BadRequest("Empleado no encontrado");

            if (dto.HoraInicio >= dto.HoraFin)
                return BadRequest("La hora de inicio debe ser menor a la hora fin");

            var motivo = new Motivo
            {
                Descripcion = dto.Descripcion,
                Fecha = dto.Fecha,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                DocumentoEmpleado = empleado.DocumentoEmpleado,
                EstadoId = 1 // Pendiente
            };

            _context.Motivos.Add(motivo);

            // ===============================
            // 🚫 Cancelar citas automáticamente
            // ===============================
            var estadoCancelado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Nombre == "Cancelado");

            if (estadoCancelado != null)
            {
                var citasACancelar = await _context.Agenda
                    .Where(c =>
                        c.DocumentoEmpleado == empleado.DocumentoEmpleado &&
                        c.FechaCita == dto.Fecha &&
                        c.HoraInicio >= dto.HoraInicio &&
                        c.HoraInicio < dto.HoraFin)
                    .ToListAsync();

                foreach (var cita in citasACancelar)
                {
                    cita.EstadoId = estadoCancelado.EstadoId;
                }
            }

            await _context.SaveChangesAsync();

            // ===============================
            // 📧 Enviar correos a empleados registrados
            // ===============================
            try
            {
                var empleadosConEmail = await _context.Empleados
                    .Include(e => e.Usuario)
                    .Where(e => e.Estado && e.Usuario.Email != null)
                    .Select(e => new { e.Usuario.Email, e.Nombre })
                    .ToListAsync();

                var emailTasks = empleadosConEmail.Select(dest =>
                    _emailService.EnviarNotificacionMotivo(
                        dest.Email!,
                        empleado.Nombre,
                        dto.Fecha,
                        dto.HoraInicio,
                        dto.HoraFin,
                        dto.Descripcion
                    )
                );

                await Task.WhenAll(emailTasks);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                Console.WriteLine($"Error enviando correos: {ex.Message}");
            }

            return Ok(new MotivoResponseDto
            {
                MotivoId = motivo.MotivoId,
                Descripcion = motivo.Descripcion,
                Fecha = motivo.Fecha,
                HoraInicio = motivo.HoraInicio,
                HoraFin = motivo.HoraFin,
                DocumentoEmpleado = motivo.DocumentoEmpleado,
                EstadoId = motivo.EstadoId
            });
        }

        // ===============================
        // 📋 GET: Todos los motivos
        // ===============================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMotivos()
        {
            var motivos = await _context.Motivos
                .Select(m => new MotivoResponseDto
                {
                    MotivoId = m.MotivoId,
                    Descripcion = m.Descripcion,
                    Fecha = m.Fecha,
                    HoraInicio = m.HoraInicio,
                    HoraFin = m.HoraFin,
                    DocumentoEmpleado = m.DocumentoEmpleado,
                    EstadoId = m.EstadoId
                })
                .ToListAsync();

            return Ok(motivos);
        }

        // ===============================
        // 🔍 GET: Motivo por ID
        // ===============================
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetMotivo(int id)
        {
            var m = await _context.Motivos.FindAsync(id);

            if (m == null)
                return NotFound();

            return Ok(new MotivoResponseDto
            {
                MotivoId = m.MotivoId,
                Descripcion = m.Descripcion,
                Fecha = m.Fecha,
                HoraInicio = m.HoraInicio,
                HoraFin = m.HoraFin,
                DocumentoEmpleado = m.DocumentoEmpleado,
                EstadoId = m.EstadoId
            });
        }

        // ===============================
        // 🔄 PUT: Aprobar / Rechazar motivo
        // ===============================
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:SuperAdmin")]
        public async Task<IActionResult> ActualizarMotivo(int id, [FromBody] MotivoUpdateDto dto)
        {
            var motivo = await _context.Motivos.FindAsync(id);

            if (motivo == null)
                return NotFound("Motivo no encontrado");

            motivo.EstadoId = dto.EstadoId;

            // ===============================
            // 🚫 Si se aprueba → cancelar citas
            // ===============================
            if (dto.EstadoId == 6) // Aprobado
            {
                var estadoCancelado = await _context.Estados
                    .FirstOrDefaultAsync(e => e.Nombre == "Cancelado");

                if (estadoCancelado != null)
                {
                    var citas = await _context.Agenda
                        .Where(c =>
                            c.DocumentoEmpleado == motivo.DocumentoEmpleado &&
                            c.FechaCita == motivo.Fecha &&
                            c.HoraInicio >= motivo.HoraInicio &&
                            c.HoraInicio < motivo.HoraFin)
                        .ToListAsync();

                    foreach (var cita in citas)
                    {
                        cita.EstadoId = estadoCancelado.EstadoId;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Motivo actualizado correctamente"
            });
        }
}
}