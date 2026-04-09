using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public VentasController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // Reutilizado patrón para obtener documento desde el token (igual que en AgendaController)
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
        // GET - Todas las ventas (Administrador, Asistente) con paginación y búsqueda
        // ---------------------------------------------------------
        [HttpGet]
        [Authorize(Policy = "perm:Ventas")]
        public async Task<ActionResult> ObtenerTodasLasVentas([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Venta
                    .Include(v => v.DocumentoClienteNavigation)
                    .Include(v => v.DetalleVentas)
                        .ThenInclude(dv => dv.Servicio)
                    .AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    
                    // Intentar parsear el total para búsqueda numérica si es posible
                    decimal totalBusqueda = 0;
                    bool esNumero = decimal.TryParse(busqueda, out totalBusqueda);

                    query = query.Where(v => 
                        v.VentaId.ToLower().Contains(busqueda) || 
                        v.DocumentoCliente.ToLower().Contains(busqueda) || 
                        v.DocumentoClienteNavigation.Nombre.ToLower().Contains(busqueda) ||
                        (esNumero && v.Total == totalBusqueda)
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var ventas = await query
                    .OrderByDescending(v => v.VentaId)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToListAsync();

                // Cargar diccionarios para otros datos (Metodos de Pago, Empleados y Agendas)
                var metodosDict = await _context.MetodoPagos
                    .ToDictionaryAsync(m => m.MetodopagoId, m => m.Nombre);

                var empleadosDict = await _context.Empleados
                    .ToDictionaryAsync(e => e.DocumentoEmpleado, e => e.Nombre);

                var ventaIds = ventas.Select(v => v.VentaId).ToList();
                var agendas = await _context.Agenda
                    .Where(a => a.VentaId != null && ventaIds.Contains(a.VentaId))
                    .Include(a => a.DocumentoEmpleadoNavigation)
                    .ToListAsync();

                var agendaPorVenta = agendas
                    .GroupBy(a => a.VentaId)
                    .ToDictionary(g => g.Key!, g => g.First());

                var resultados = ventas.Select(v =>
                {
                    agendaPorVenta.TryGetValue(v.VentaId, out var ag);
                    metodosDict.TryGetValue(v.MetodopagoId, out var metodoNombre);
                    
                    // Priorizar el empleado guardado en la venta, sino usar el de la agenda
                    var docEmpleado = !string.IsNullOrEmpty(v.DocumentoEmpleado) ? v.DocumentoEmpleado : ag?.DocumentoEmpleado ?? string.Empty;
                    empleadosDict.TryGetValue(docEmpleado, out var nombreEmpleado);

                    return new VentaDto
                    {
                        VentaId = v.VentaId,
                        DocumentoCliente = v.DocumentoCliente,
                        ClienteNombre = v.DocumentoClienteNavigation?.Nombre ?? string.Empty,
                        EmpleadoDocumento = docEmpleado,
                        EmpleadoNombre = nombreEmpleado ?? ag?.DocumentoEmpleadoNavigation?.Nombre ?? string.Empty,
                        MetodopagoId = v.MetodopagoId,
                        MetodoPago = metodoNombre ?? string.Empty,
                        Subtotal = v.Subtotal,
                        Total = v.Total,
                        Estado = v.Estado,
                        Observacion = v.observacion ?? string.Empty,
                        Servicios = v.DetalleVentas.Select(dv => new VentaServicioDto
                        {
                            ServicioId = dv.ServicioId,
                            Nombre = dv.Servicio.Nombre,
                            Precio = dv.Precio
                        }).ToList()
                    };
                }).ToList();

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
                return StatusCode(500, new { success = false, message = $"Error al obtener ventas: {ex.Message}" });
            }
        }

        // ---------------------------------------------------------
        // GET - Obtener venta por id (Administrador, Asistente, Cliente)
        // ---------------------------------------------------------
        [HttpGet("{ventaId}")]
        [Authorize(Policy = "perm:Ventas")]
        public async Task<ActionResult> ObtenerVenta(string ventaId)
        {
            try
            {
                var venta = await _context.Venta
                    .Include(v => v.DetalleVentas)
                        .ThenInclude(dv => dv.Servicio)
                    .FirstOrDefaultAsync(v => v.VentaId == ventaId);

                if (venta == null)
                    return NotFound(new { success = false, message = "Venta no encontrada" });

                // Si es cliente, validar que la venta pertenezca a su documento
                var rol = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rol == "Cliente")
                {
                    var documentoCliente = await ObtenerDocumentoDesdeTokenAsync();
                    if (documentoCliente == null || documentoCliente != venta.DocumentoCliente)
                        return Forbid();
                }

                var clienteNombre = await _context.Clientes
                    .Where(c => c.DocumentoCliente == venta.DocumentoCliente)
                    .Select(c => c.Nombre)
                    .FirstOrDefaultAsync() ?? string.Empty;

                var metodoNombre = await _context.MetodoPagos
                    .Where(m => m.MetodopagoId == venta.MetodopagoId)
                    .Select(m => m.Nombre)
                    .FirstOrDefaultAsync() ?? string.Empty;

                // Buscar agenda asociada para obtener empleado
                var agenda = await _context.Agenda
                    .Include(a => a.DocumentoEmpleadoNavigation)
                    .FirstOrDefaultAsync(a => a.VentaId == ventaId);

                // Obtener nombre del empleado (priorizando el de la venta si existe)
                var docEmpleado = !string.IsNullOrEmpty(venta.DocumentoEmpleado) ? venta.DocumentoEmpleado : agenda?.DocumentoEmpleado ?? string.Empty;
                var nombreEmpleado = string.Empty;

                if (!string.IsNullOrEmpty(docEmpleado))
                {
                    nombreEmpleado = await _context.Empleados
                        .Where(e => e.DocumentoEmpleado == docEmpleado)
                        .Select(e => e.Nombre)
                        .FirstOrDefaultAsync() ?? agenda?.DocumentoEmpleadoNavigation?.Nombre ?? string.Empty;
                }

                var dto = new VentaDto
                {
                    VentaId = venta.VentaId,
                    DocumentoCliente = venta.DocumentoCliente,
                    ClienteNombre = clienteNombre,
                    EmpleadoDocumento = docEmpleado,
                    EmpleadoNombre = nombreEmpleado,
                    MetodopagoId = venta.MetodopagoId,
                    MetodoPago = metodoNombre,
                    Subtotal = venta.Subtotal,
                    Total = venta.Total,
                    Estado = venta.Estado,
                    Observacion = venta.observacion ?? string.Empty,
                    Servicios = venta.DetalleVentas.Select(dv => new VentaServicioDto
                    {
                        ServicioId = dv.ServicioId,
                        Nombre = dv.Servicio.Nombre,
                        Precio = dv.Precio
                    }).ToList()
                };

                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al obtener venta: {ex.Message}" });
            }
        }

        // ---------------------------------------------------------
        // POST - Crear venta (Administrador, Asistente)
        // ---------------------------------------------------------
        [HttpPost]
        [Authorize(Policy = "perm:Ventas")]
        public async Task<ActionResult> CrearVenta([FromBody] CrearVentaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar cliente
                var clienteExiste = await _context.Clientes
                    .AnyAsync(c => c.DocumentoCliente == dto.DocumentoCliente);

                if (!clienteExiste)
                    return BadRequest(new { success = false, message = "Cliente no encontrado" });

                // Validar método de pago
                var metodo = await _context.MetodoPagos.FindAsync(dto.MetodopagoId);
                if (metodo == null)
                    return BadRequest(new { success = false, message = "Método de pago no válido" });

                // Generar VentaId con formato "VEN-NNN" (3 dígitos aleatorios) y asegurar unicidad
                string ventaId;
                var rng = new Random();
                int attempts = 0;
                do
                {
                    var numero = rng.Next(0, 1000).ToString("D3");
                    ventaId = $"VEN-{numero}";
                    attempts++;
                } while (await _context.Venta.AnyAsync(v => v.VentaId == ventaId) && attempts < 10);

                if (await _context.Venta.AnyAsync(v => v.VentaId == ventaId))
                {
                    return StatusCode(500, new { success = false, message = "No fue posible generar un ID de venta único. Intente nuevamente." });
                }

                // Crear venta
                var venta = new Ventum
                {
                    VentaId = ventaId,
                    DocumentoCliente = dto.DocumentoCliente,
                    DocumentoEmpleado = dto.DocumentoEmpleado,
                    MetodopagoId = dto.MetodopagoId,
                    Subtotal = dto.Subtotal,
                    Total = dto.Total,
                    Estado = true,
                    observacion = dto.Observacion ?? string.Empty
                };

                _context.Venta.Add(venta);

                // Agregar detalles de venta
                if (dto.Detalles != null && dto.Detalles.Any())
                {
                    foreach (var detalleDto in dto.Detalles)
                    {
                        var detalle = new DetalleVenta
                        {
                            VentaId = ventaId,
                            ServicioId = detalleDto.ServicioId,
                            Precio = detalleDto.Precio
                        };
                        _context.DetalleVentas.Add(detalle);
                    }
                }

                // Si se proporcionaron IDs de agendas, asociar la venta a esas citas
                if (dto.AgendasIds != null && dto.AgendasIds.Any())
                {
                    var agendas = await _context.Agenda
                        .Where(a => dto.AgendasIds.Contains(a.AgendaId))
                        .ToListAsync();

                    foreach (var agenda in agendas)
                    {
                        agenda.VentaId = ventaId;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Venta creada correctamente", ventaId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error al crear venta: {ex.Message}" });
            }
        }

        // ---------------------------------------------------------
        // PUT - Actualizar venta (Administrador, Asistente)
        // Sólo permite editar descuento y estado
        // ---------------------------------------------------------
        [HttpPut("{ventaId}")]
        [Authorize(Policy = "perm:Ventas")]
        public async Task<ActionResult> ActualizarVenta(string ventaId, [FromBody] ActualizarVentaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = await _context.Venta.FirstOrDefaultAsync(v => v.VentaId == ventaId);
                if (venta == null)
                    return NotFound(new { success = false, message = "Venta no encontrada" });

                // Aplicar estado si se proporciona
                if (dto.Estado.HasValue)
                {
                    venta.Estado = dto.Estado.Value;
                    venta.observacion = dto.Observacion ?? venta.observacion; // Actualizar observación si se proporciona
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Venta actualizada correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error al actualizar venta: {ex.Message}" });
            }
        }
    }
}