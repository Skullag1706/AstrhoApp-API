using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AgendaController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public AgendaController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------
        //  Helpers
        // ---------------------------------------------------------
        private static string DiaSemanaEnEspañol(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miercoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sabado",
                DayOfWeek.Sunday => "Domingo",
                _ => ""
            };
        }

        private static string NormalizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "";

            nombre = nombre.Trim().ToLower()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u");

            return nombre;
        }

        private static List<TimeOnly> GenerarHorasEnIntervalos(TimeOnly inicio, TimeOnly fin, int minutosPaso = 30)
        {
            var resultado = new List<TimeOnly>();

            var actual = inicio.ToTimeSpan();
            var limite = fin.ToTimeSpan();

            while (actual < limite)
            {
                resultado.Add(TimeOnly.FromTimeSpan(actual));
                actual = actual.Add(TimeSpan.FromMinutes(minutosPaso));
            }

            return resultado;
        }

        private async Task<bool> EmpleadoTieneRangoHorarioAsync(
            string documentoEmpleado, DateOnly fecha, TimeOnly horaCita)
        {
            var diaNombre = DiaSemanaEnEspañol(fecha.DayOfWeek);
            var diaNorm = NormalizarNombre(diaNombre);

            // TRAER desde DB primero
            var horariosDias = await _context.HorarioEmpleados
                .Include(he => he.HorarioDia)
                    .ThenInclude(hd => hd.Horario)
                .Where(he => he.DocumentoEmpleado == documentoEmpleado && he.HorarioDia.Horario.Estado)
                .Select(he => he.HorarioDia)
                .ToListAsync();

            // FILTRO EN MEMORIA
            return horariosDias.Any(hd =>
            {
                var diaHorario = NormalizarNombre(hd.DiaSemana);
                return diaHorario == diaNorm &&
                       hd.HoraInicio <= horaCita &&
                       horaCita < hd.HoraFin;
            });
        }
        private async Task<string?> ObtenerDocumentoDesdeTokenAsync()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            if (usuarioIdClaim == null || rol == null)
                return null;

            var usuarioId = int.Parse(usuarioIdClaim);

            if (rol == "Cliente")
            {
                return await _context.Clientes
                    .Where(c => c.UsuarioId == usuarioId)
                    .Select(c => c.DocumentoCliente)
                    .FirstOrDefaultAsync();
            }

            // Admin o Asistente → Empleado
            return await _context.Empleados
                .Where(e => e.UsuarioId == usuarioId)
                .Select(e => e.DocumentoEmpleado)
                .FirstOrDefaultAsync();
        }


        // ---------------------------------------------------------
        //  GET - Todas las citas
        // ---------------------------------------------------------
        [HttpGet]
        [Authorize(Policy = "perm:Agenda")]
        public async Task<ActionResult> ObtenerTodasLasCitas([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Agenda
                    .Include(a => a.Estado)
                    .Include(a => a.Metodopago)
                    .Include(a => a.DocumentoClienteNavigation)
                    .Include(a => a.DocumentoEmpleadoNavigation)
                    .AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(a => 
                        a.DocumentoCliente.ToLower().Contains(busqueda) || 
                        a.DocumentoEmpleado.ToLower().Contains(busqueda) ||
                        (a.DocumentoClienteNavigation != null && a.DocumentoClienteNavigation.Nombre.ToLower().Contains(busqueda)) ||
                        (a.DocumentoEmpleadoNavigation != null && a.DocumentoEmpleadoNavigation.Nombre.ToLower().Contains(busqueda)) ||
                        (a.Estado != null && a.Estado.Nombre.ToLower().Contains(busqueda))
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var citas = await query
                    .OrderBy(a => a.FechaCita)
                    .ThenBy(a => a.HoraInicio)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToListAsync();

                var resultados = new List<object>();

                foreach (var cita in citas)
                {
                    var servicios = await _context.ServicioAgenda
                        .Include(sa => sa.Servicio)
                        .Where(sa => sa.AgendaId == cita.AgendaId)
                        .Select(sa => sa.Servicio.Nombre)
                        .ToListAsync();

                    resultados.Add(new
                    {
                        cita.AgendaId,
                        cita.DocumentoCliente,
                        Cliente = cita.DocumentoClienteNavigation?.Nombre,
                        cita.DocumentoEmpleado,
                        Empleado = cita.DocumentoEmpleadoNavigation?.Nombre,
                        cita.FechaCita,
                        cita.HoraInicio,
                        Estado = cita.Estado?.Nombre,
                        MetodoPago = cita.Metodopago?.Nombre,
                        Servicios = servicios
                    });
                }

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = resultados 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al obtener citas: {ex.Message}" });
            }
        }
        // ---------------------------------------------------------
        //  POST - Crear Cita
        //  Nota: si quieres que Cliente pueda crear sin claim, registra una policy combinada en Program.cs (ver snippet)
        // ---------------------------------------------------------
        [HttpPost]
        [Authorize(Policy = "perm:Agenda")]
        public async Task<ActionResult> CrearCita([FromBody] CrearCitaDto dto)
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rol == "Cliente")
            {
                var documentoCliente = await ObtenerDocumentoDesdeTokenAsync();

                if (documentoCliente == null)
                    return Unauthorized();

                dto.DocumentoCliente = documentoCliente;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar cliente
                var clienteExiste = await _context.Clientes
                    .AnyAsync(c => c.DocumentoCliente == dto.DocumentoCliente);

                if (!clienteExiste)
                    return BadRequest(new { success = false, message = "Cliente no encontrado" });

                // Validar empleado
                var empleadoExiste = await _context.Empleados
                    .AnyAsync(e => e.DocumentoEmpleado == dto.DocumentoEmpleado);

                if (!empleadoExiste)
                    return BadRequest(new { success = false, message = "Empleado no encontrado" });

                // Validar que esté dentro del horario
                var tieneHorarioValido = await EmpleadoTieneRangoHorarioAsync(
                    dto.DocumentoEmpleado, dto.FechaCita, dto.HoraInicio);

                if (!tieneHorarioValido)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El empleado no tiene horario asignado para ese día y hora."
                    });
                }

                // Validación de disponibilidad
                var estadoCancelado = await _context.Estados
                    .FirstOrDefaultAsync(e => e.Nombre == "Cancelado");

                if (estadoCancelado == null)
                    return BadRequest(new { success = false, message = "Estado 'Cancelado' no configurado" });

                var ocupada = await _context.Agenda.AnyAsync(a =>
                    a.DocumentoEmpleado == dto.DocumentoEmpleado &&
                    a.FechaCita == dto.FechaCita &&
                    a.HoraInicio == dto.HoraInicio &&
                    a.EstadoId != estadoCancelado.EstadoId
                );

                if (ocupada)
                {
                    return BadRequest(new { success = false, message = "La hora seleccionada ya está ocupada" });
                }

                // Validar servicios seleccionados
                if (dto.ServiciosIds == null || !dto.ServiciosIds.Any())
                    return BadRequest(new { success = false, message = "Debe seleccionar al menos un servicio" });

                foreach (var servicioId in dto.ServiciosIds)
                {
                    var servicio = await _context.Servicios.FindAsync(servicioId);
                    if (servicio == null)
                        return BadRequest(new { success = false, message = $"Servicio {servicioId} no encontrado" });
                    if (!servicio.Estado)
                        return BadRequest(new { success = false, message = $"El servicio '{servicio.Nombre}' está inactivo" });
                }

                // Validar método de pago
                var metodo = await _context.MetodoPagos.FindAsync(dto.MetodoPagoId);
                if (metodo == null)
                    return BadRequest(new { success = false, message = "Método de pago no válido" });

                // Crear cita
                var cita = new Agendum
                {
                    DocumentoCliente = dto.DocumentoCliente,
                    DocumentoEmpleado = dto.DocumentoEmpleado,
                    FechaCita = dto.FechaCita,
                    HoraInicio = dto.HoraInicio,
                    MetodopagoId = dto.MetodoPagoId,
                    EstadoId = await _context.Estados
                        .Where(e => e.Nombre == "Pendiente")
                        .Select(e => e.EstadoId)
                        .FirstAsync(),
                    Observaciones = dto.Observaciones
                };

                await _context.Agenda.AddAsync(cita);
                await _context.SaveChangesAsync();

                // Asociar servicios
                foreach (var servicioId in dto.ServiciosIds)
                {
                    _context.ServicioAgenda.Add(new ServicioAgendum
                    {
                        AgendaId = cita.AgendaId,
                        ServicioId = servicioId
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Cita creada correctamente", citaId = cita.AgendaId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error al crear cita: {ex.Message}" });
            }
        }

        // ---------------------------------------------------------
        //  PUT - Actualizar Cita
        //  Policy: perm:Agenda.Update
        // ---------------------------------------------------------
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Agenda")]
        public async Task<ActionResult> ActualizarCita(int id, [FromBody] ActualizarCitaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cita = await _context.Agenda
                    .Include(a => a.Estado)
                    .FirstOrDefaultAsync(a => a.AgendaId == id);

                if (cita == null)
                    return NotFound();

                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol == "Cliente")
                {
                    var documentoCliente = await ObtenerDocumentoDesdeTokenAsync();

                    if (documentoCliente != cita.DocumentoCliente)
                        return Forbid();
                }

                if (cita == null)
                    return NotFound(new { success = false, message = "Cita no encontrada" });

                if (cita.Estado.Nombre == "Completada")
                    return BadRequest(new { success = false, message = "No se puede editar una cita completada" });

                // Validar horario
                var valido = await EmpleadoTieneRangoHorarioAsync(
                    dto.DocumentoEmpleado, dto.FechaCita, dto.HoraInicio);

                if (!valido)
                {
                    return BadRequest(new { success = false, message = "El empleado no tiene horario para esa fecha/hora" });
                }

                // Validar disponibilidad
                var estadoCancelado = await _context.Estados
                    .FirstAsync(e => e.Nombre == "Cancelado");

                var ocupada = await _context.Agenda.AnyAsync(a =>
                    a.AgendaId != id &&
                    a.DocumentoEmpleado == dto.DocumentoEmpleado &&
                    a.FechaCita == dto.FechaCita &&
                    a.HoraInicio == dto.HoraInicio &&
                    a.EstadoId != estadoCancelado.EstadoId
                );

                if (ocupada)
                {
                    return BadRequest(new { success = false, message = "La hora ya está ocupada" });
                }

                // Actualizar
                cita.DocumentoCliente = dto.DocumentoCliente;
                cita.DocumentoEmpleado = dto.DocumentoEmpleado;
                cita.FechaCita = dto.FechaCita;
                cita.HoraInicio = dto.HoraInicio;
                cita.Observaciones = dto.Observaciones;

                if (dto.EstadoId.HasValue)
                    cita.EstadoId = dto.EstadoId.Value;

                if (dto.MetodoPagoId.HasValue)
                    cita.MetodopagoId = dto.MetodoPagoId.Value;

                // Remover servicios anteriores
                var serviciosPrevios = await _context.ServicioAgenda
                    .Where(sa => sa.AgendaId == id)
                    .ToListAsync();

                if (serviciosPrevios.Any())
                    _context.ServicioAgenda.RemoveRange(serviciosPrevios);

                // Agregar nuevos
                if (dto.ServiciosIds != null)
                {
                    foreach (var servicioId in dto.ServiciosIds)
                    {
                        _context.ServicioAgenda.Add(new ServicioAgendum
                        {
                            AgendaId = id,
                            ServicioId = servicioId
                        });
                    }
                }

                // Si el estado solicitado es "Completada" (o variante) y aún no existe venta -> crear venta
                bool ventaCreada = false;
                string? ventaId = null;

                if (dto.EstadoId.HasValue)
                {
                    var estadoNuevo = await _context.Estados.FindAsync(dto.EstadoId.Value);
                    if (estadoNuevo != null &&
                        estadoNuevo.Nombre != null &&
                        estadoNuevo.Nombre.StartsWith("Complet", StringComparison.OrdinalIgnoreCase) &&
                        string.IsNullOrWhiteSpace(cita.VentaId))
                    {
                        // Determinar método de pago para la venta
                        var metodopagoIdParaVenta = dto.MetodoPagoId.HasValue ? dto.MetodoPagoId.Value : cita.MetodopagoId;
                        var metodoValido = await _context.MetodoPagos.FindAsync(metodopagoIdParaVenta);
                        if (metodoValido == null)
                        {
                            return BadRequest(new { success = false, message = "Método de pago para la venta no válido" });
                        }

                        // Calcular subtotal: si se enviaron servicios nuevos usamos esos ids; si no, sumamos los servicios actuales en DB
                        decimal subtotal = 0m;
                        if (dto.ServiciosIds != null && dto.ServiciosIds.Any())
                        {
                            subtotal = await _context.Servicios
                                .Where(s => dto.ServiciosIds.Contains(s.ServicioId))
                                .SumAsync(s => s.Precio);
                        }
                        else
                        {
                            subtotal = await _context.ServicioAgenda
                                .Where(sa => sa.AgendaId == id)
                                .Select(sa => sa.Servicio.Precio)
                                .SumAsync();
                        }

                        // Generar VentaId con formato "VEN-NNN" (3 dígitos aleatorios) y asegurar unicidad
                        string ventaIdCandidate;
                        var rng = new Random();
                        int attempts = 0;
                        do
                        {
                            var numero = rng.Next(0, 1000).ToString("D3");
                            ventaIdCandidate = $"VEN-{numero}";
                            attempts++;
                        } while (await _context.Venta.AnyAsync(v => v.VentaId == ventaIdCandidate) && attempts < 10);

                        if (await _context.Venta.AnyAsync(v => v.VentaId == ventaIdCandidate))
                        {
                            return StatusCode(500, new { success = false, message = "No fue posible generar un ID de venta único. Intente nuevamente." });
                        }

                        ventaId = ventaIdCandidate;

                        var venta = new Ventum
                        {
                            VentaId = ventaId,
                            DocumentoCliente = cita.DocumentoCliente,
                            DocumentoEmpleado = cita.DocumentoEmpleado, // Agregado para consistencia
                            MetodopagoId = metodopagoIdParaVenta,
                            Subtotal = subtotal,
                            Total = subtotal,
                            Estado = true,
                            // La observación de la venta se toma del campo de la entidad venta: se copia desde la observación de la cita
                            observacion = cita.Observaciones
                        };

                        _context.Venta.Add(venta);

                        // Crear detalles de venta a partir de los servicios de la cita
                        var serviciosIdsParaDetalle = dto.ServiciosIds != null && dto.ServiciosIds.Any() 
                            ? dto.ServiciosIds 
                            : await _context.ServicioAgenda
                                .Where(sa => sa.AgendaId == id)
                                .Select(sa => sa.ServicioId)
                                .ToListAsync();

                        foreach (var sId in serviciosIdsParaDetalle)
                        {
                            var servicio = await _context.Servicios.FindAsync(sId);
                            if (servicio != null)
                            {
                                var detalle = new DetalleVenta
                                {
                                    VentaId = ventaId,
                                    ServicioId = sId,
                                    Precio = servicio.Precio
                                };
                                _context.DetalleVentas.Add(detalle);
                            }
                        }

                        // Asociar la venta a la cita
                        cita.VentaId = ventaId;
                        ventaCreada = true;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (ventaCreada)
                {
                    return Ok(new { success = true, message = "Cita actualizada correctamente. Venta creada.", ventaId });
                }

                return Ok(new { success = true, message = "Cita actualizada correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error al actualizar cita: {ex.Message}" });
            }
        }

        // ---------------------------------------------------------
        //  GET - Horas disponibles
        //  Policy: perm:Agenda.Availability
        // ---------------------------------------------------------
        [HttpGet("horas-disponibles")]
        [Authorize(Policy = "perm:Agenda")]
        public async Task<ActionResult> ObtenerHorasDisponibles(
            [FromQuery] string documentoEmpleado,
            [FromQuery] DateOnly? fecha = null,
            [FromQuery] int minutosPaso = 30)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentoEmpleado))
                    return BadRequest(new { success = false, message = "Debe proporcionar documentoEmpleado" });

                var fechaConsulta = fecha ?? DateOnly.FromDateTime(DateTime.Now);

                var diaNombre = DiaSemanaEnEspañol(fechaConsulta.DayOfWeek);
                var diaNorm = NormalizarNombre(diaNombre);

                // TRAER horarios primero
                var horariosAsignados = await _context.HorarioEmpleados
                    .Include(he => he.HorarioDia)
                        .ThenInclude(hd => hd.Horario)
                    .Where(he => he.DocumentoEmpleado == documentoEmpleado && he.HorarioDia.Horario.Estado)
                    .Select(he => he.HorarioDia)
                    .ToListAsync();

                // FILTRO EN MEMORIA
                var horariosDelDia = horariosAsignados
                    .Where(hd => NormalizarNombre(hd.DiaSemana) == diaNorm)
                    .ToList();

                if (!horariosDelDia.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El empleado no tiene horario asignado para este día."
                    });
                }

                // Generar intervalos
                var intervalos = new List<TimeOnly>();
                foreach (var h in horariosDelDia)
                {
                    var generados = GenerarHorasEnIntervalos(h.HoraInicio, h.HoraFin, minutosPaso);
                    intervalos.AddRange(generados);
                }

                intervalos = intervalos.Distinct().OrderBy(h => h).ToList();

                // Obtener citas ocupadas
                var estadoCancelado = await _context.Estados
                    .FirstAsync(e => e.Nombre == "Cancelado");

                var horasOcupadas = await _context.Agenda
                    .Where(a =>
                        a.DocumentoEmpleado == documentoEmpleado &&
                        a.FechaCita == fechaConsulta &&
                        a.EstadoId != estadoCancelado.EstadoId)
                    .Select(a => a.HoraInicio)
                    .ToListAsync();

                var disponibles = intervalos
                    .Where(h => !horasOcupadas.Contains(h))
                    .ToList();

                return Ok(new
                {
                    success = true,
                    fecha = fechaConsulta,
                    dia = diaNombre,
                    horasDisponibles = disponibles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al obtener horas: {ex.Message}" });
            }
        }

        // ---------------------------------------------------------
        //  GET - Buscar citas por fecha
        //  Policy: perm:Agenda.View
        // ---------------------------------------------------------
        [HttpGet("por-fecha")]
        [Authorize(Policy = "perm:Agenda")]
        public async Task<ActionResult> BuscarCitasPorFecha([FromQuery] DateOnly fecha)
        {
            var citas = await _context.Agenda
                .Include(a => a.Estado)
                .Include(a => a.Metodopago)
                .Include(a => a.DocumentoClienteNavigation)
                .Include(a => a.DocumentoEmpleadoNavigation)
                .Where(a => a.FechaCita == fecha)
                .ToListAsync();

            if (!citas.Any())
                return NotFound(new
                {
                    success = false,
                    message = "No se encontraron citas para la fecha indicada"
                });

            var resultado = new List<object>();

            foreach (var cita in citas)
            {
                var servicios = await _context.ServicioAgenda
                    .Include(sa => sa.Servicio)
                    .Where(sa => sa.AgendaId == cita.AgendaId)
                    .Select(sa => sa.Servicio.Nombre)
                    .ToListAsync();

                resultado.Add(new
                {
                    cita.AgendaId,
                    cita.DocumentoCliente,
                    Cliente = cita.DocumentoClienteNavigation?.Nombre,
                    cita.DocumentoEmpleado,
                    Empleado = cita.DocumentoEmpleadoNavigation?.Nombre,
                    cita.FechaCita,
                    cita.HoraInicio,
                    Estado = cita.Estado?.Nombre,
                    MetodoPago = cita.Metodopago?.Nombre,
                    Servicios = servicios
                });
            }

            return Ok(new
            {
                success = true,
                total = resultado.Count,
                data = resultado
            });
        }

        // ---------------------------------------------------------
        //  DELETE - Eliminar cita
        //  Policy: perm:Agenda.Delete
        // ---------------------------------------------------------
        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:Agenda")]
        public async Task<ActionResult> EliminarCita(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cita = await _context.Agenda
                    .Include(a => a.Estado)
                    .FirstOrDefaultAsync(a => a.AgendaId == id);

                if (cita == null)
                    return NotFound();

                var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                if (rol == "Cliente")
                {
                    var documentoCliente = await ObtenerDocumentoDesdeTokenAsync();

                    if (documentoCliente != cita.DocumentoCliente)
                        return Forbid();
                }

                // Remover relaciones dependientes (ServicioAgenda) antes de eliminar la cita
                var serviciosPrevios = await _context.ServicioAgenda
                    .Where(sa => sa.AgendaId == id)
                    .ToListAsync();

                if (serviciosPrevios.Any())
                    _context.ServicioAgenda.RemoveRange(serviciosPrevios);

                _context.Agenda.Remove(cita);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { success = true, message =  "Cita eliminada correctamente"});
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var inner = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { success = false, message = $"Error al eliminar cita: {inner}" });
            }
        }

        // ---------------------------------------------------------
        //  GET - Mis citas - cliente
        // ---------------------------------------------------------
        [Authorize(Roles = "Cliente")]
        [HttpGet("mis-citas")]
        public async Task<IActionResult> MisCitas()
        {
            var documentoCliente = await ObtenerDocumentoDesdeTokenAsync();
            if (documentoCliente == null)
                return Unauthorized();

            // Traer las citas completas (para poder obtener navegaciones si se necesitan)
            var citasEntidades = await _context.Agenda
                .Include(a => a.DocumentoEmpleadoNavigation)
                .Include(a => a.Estado)
                .Include(a => a.Metodopago) // incluir método de pago
                .Where(a => a.DocumentoCliente == documentoCliente)
                .OrderBy(a => a.FechaCita)
                .ThenBy(a => a.HoraInicio)
                .ToListAsync();

            var citas = new List<CitaClienteDto>();

            foreach (var cita in citasEntidades)
            {
                var servicios = await _context.ServicioAgenda
                    .Include(sa => sa.Servicio)
                    .Where(sa => sa.AgendaId == cita.AgendaId)
                    .Select(sa => sa.Servicio.Nombre)
                    .ToListAsync();

                citas.Add(new CitaClienteDto
                {
                    AgendaId = cita.AgendaId,
                    FechaCita = cita.FechaCita,
                    HoraInicio = cita.HoraInicio,
                    DocumentoEmpleado = cita.DocumentoEmpleado,
                    NombreEmpleado = cita.DocumentoEmpleadoNavigation?.Nombre ?? string.Empty,
                    MetodoPago = cita.Metodopago?.Nombre ?? string.Empty,
                    Estado = cita.Estado?.Nombre ?? string.Empty,
                    Servicios = servicios,
                });
            }

            return Ok(new { success = true, data = citas });
        }

    }
}
