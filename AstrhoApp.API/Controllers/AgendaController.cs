using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgendaController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public AgendaController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        [HttpGet("disponibilidad")]
        public async Task<ActionResult> ObtenerDisponibilidad([FromQuery] DateTime fecha)
        {
            var fechaConsulta = DateOnly.FromDateTime(fecha);

            var citas = await _context.Agenda
                .Where(a => a.FechaCita == fechaConsulta)
                .Select(a => new
                {
                    a.HoraInicio,
                    a.HoraFin,
                    a.DocumentoEmpleado
                })
                .ToListAsync();

            return Ok(citas);
        }

        [HttpPost]
        public async Task<ActionResult<AgendaResponseDto>> CrearAgenda([FromBody] CrearAgendaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validar cliente existe
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.DocumentoCliente == dto.DocumentoCliente);
                if (!clienteExiste)
                    return BadRequest("Cliente no encontrado");

                // Validar empleado existe
                var empleadoExiste = await _context.Empleados.AnyAsync(e => e.DocumentoEmpleado == dto.DocumentoEmpleado);
                if (!empleadoExiste)
                    return BadRequest("Empleado no encontrado");

                // Crear agenda
                var agenda = new Agendum
                {
                    DocumentoCliente = dto.DocumentoCliente,
                    DocumentoEmpleado = dto.DocumentoEmpleado,
                    FechaCita = dto.FechaCita,
                    HoraInicio = dto.HoraInicio,
                    HoraFin = dto.HoraFin,
                    Estado = "Pendiente",
                    MetodoPago = dto.MetodoPago,
                    Observaciones = dto.Observaciones
                };

                _context.Agenda.Add(agenda);
                await _context.SaveChangesAsync();

                // Agregar servicios
                foreach (var servicioId in dto.ServiciosIds)
                {
                    var servicioAgenda = new ServicioAgendum
                    {
                        ServicioId = servicioId,
                        AgendaId = agenda.AgendaId
                    };
                    _context.ServicioAgenda.Add(servicioAgenda);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new AgendaResponseDto
                {
                    AgendaId = agenda.AgendaId,
                    FechaCita = agenda.FechaCita,
                    HoraInicio = agenda.HoraInicio,
                    HoraFin = agenda.HoraFin,
                    Estado = agenda.Estado
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("mis-citas/{documentoCliente}")]
        public async Task<ActionResult> ObtenerMisCitas(int documentoCliente)
        {
            var citas = await _context.Agenda
                .Include(a => a.DocumentoClienteNavigation)
                .Include(a => a.DocumentoEmpleadoNavigation)
                .Where(a => a.DocumentoCliente == documentoCliente)
                .Select(a => new AgendaResponseDto
                {
                    AgendaId = a.AgendaId,
                    FechaCita = a.FechaCita,
                    HoraInicio = a.HoraInicio,
                    HoraFin = a.HoraFin,
                    Estado = a.Estado,
                    ClienteNombre = a.DocumentoClienteNavigation.Nombre,
                    EmpleadoNombre = a.DocumentoEmpleadoNavigation.Nombre + " " + a.DocumentoEmpleadoNavigation.Apellido
                })
                .ToListAsync();

            return Ok(citas);
        }
    }
}