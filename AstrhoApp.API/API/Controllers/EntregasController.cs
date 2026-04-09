using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EntregasController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public EntregasController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // Helper: mapea estado a nombre legible
        private static string ObtenerNombreEstado(Entregainsumo e)
        {
            return e.Estado?.Nombre ?? "Pendiente";
        }

        // ============================================================
        // LISTAR TODAS LAS ENTREGAS
        // ============================================================
        [HttpGet]
        [Authorize(Policy = "perm:Entregas")]
        public async Task<ActionResult> ListarEntregas([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Entregainsumos
                    .Include(e => e.DetalleEntregas)
                    .Include(e => e.Estado)
                    .AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(e => 
                        e.DocumentoEmpleado.ToLower().Contains(busqueda) || 
                        e.EntregainsumoId.ToString().Contains(busqueda)
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var entregas = await query
                    .OrderByDescending(e => e.FechaCreado)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToListAsync();

                var result = entregas.Select(e => new EntregaListDto
                {
                    EntregainsumoId = e.EntregainsumoId,
                    UsuarioId = e.UsuarioId,
                    DocumentoEmpleado = e.DocumentoEmpleado,
                    FechaCreado = e.FechaCreado ?? DateTime.MinValue,
                    FechaEntrega = e.FechaEntrega,
                    FechaCompletado = e.FechaCompletado,
                    EstadoId = e.EstadoId,
                    Estado = ObtenerNombreEstado(e),
                    CantidadItems = e.DetalleEntregas?.Count ?? 0,
                }).ToList();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al listar entregas: {ex.Message}"
                });
            }
        }

        // ============================================================
        // BUSCAR ENTREGA POR ID
        // ============================================================
        [HttpGet("{id}")]
        [Authorize(Policy = "perm:Entregas")]
        public async Task<ActionResult<EntregaResponseDto>> BuscarEntrega(int id)
        {
            try
            {
                var entrega = await _context.Entregainsumos
                    .Include(e => e.DetalleEntregas)
                    .Include(e => e.Estado)
                    .FirstOrDefaultAsync(e => e.EntregainsumoId == id);

                if (entrega == null)
                    return NotFound(new { success = false, message = "Entrega no encontrada" });

                var resp = new EntregaResponseDto
                {
                    EntregainsumoId = entrega.EntregainsumoId,
                    UsuarioId = entrega.UsuarioId,
                    DocumentoEmpleado = entrega.DocumentoEmpleado,
                    FechaCreado = entrega.FechaCreado ?? DateTime.MinValue,
                    FechaEntrega = entrega.FechaEntrega,
                    FechaCompletado = entrega.FechaCompletado,
                    EstadoId = entrega.EstadoId,
                    Estado = ObtenerNombreEstado(entrega),
                    Detalles = entrega.DetalleEntregas?.Select(d => new DetalleEntregaItemDto
                    {
                        InsumoId = d.InsumoId,
                        Cantidad = d.Cantidad
                    }).ToList() ?? new List<DetalleEntregaItemDto>(),
                };

                return Ok(resp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar entrega: {ex.Message}"
                });
            }
        }

        // ============================================================
        // CREAR NUEVA ENTREGA
        // ============================================================
        [HttpPost]
        [Authorize(Policy = "perm:Entregas")]
        public async Task<ActionResult<EntregaResponseDto>> CrearEntrega([FromBody] CrearEntregaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validaciones b�sicas
                if (string.IsNullOrWhiteSpace(dto.DocumentoEmpleado))
                    return BadRequest(new { success = false, message = "DocumentoEmpleado es obligatorio" });

                // Validar empleado existe
                var empleadoExiste = await _context.Empleados.AnyAsync(e => e.DocumentoEmpleado == dto.DocumentoEmpleado);
                if (!empleadoExiste)
                    return BadRequest(new { success = false, message = "Empleado no encontrado" });

                // Validar detalles
                if (dto.Detalles == null || !dto.Detalles.Any())
                    return BadRequest(new { success = false, message = "Debe incluir al menos un detalle" });

                // Validar insumos y stock antes de crear
                foreach (var item in dto.Detalles)
                {
                    if (item.Cantidad <= 0)
                        return BadRequest(new { success = false, message = "Las cantidades deben ser mayores a 0" });

                    var insumo = await _context.Insumos.FindAsync(item.InsumoId);
                    if (insumo == null)
                        return BadRequest(new { success = false, message = $"Insumo {item.InsumoId} no encontrado" });

                    if (insumo.Stock < item.Cantidad)
                        return BadRequest(new { success = false, message = $"Stock insuficiente para Insumo {item.InsumoId}. Disponible: {insumo.Stock}, requerido: {item.Cantidad}" });
                }

                // Obtener estado "Pendiente"
                var estadoPendiente = await _context.Estados.FirstOrDefaultAsync(e => e.Nombre == "Pendiente");
                if (estadoPendiente == null)
                {
                    estadoPendiente = new Estado { Nombre = "Pendiente" };
                    _context.Estados.Add(estadoPendiente);
                    await _context.SaveChangesAsync();
                }

                var entrega = new Entregainsumo
                {
                    UsuarioId = dto.UsuarioId,
                    DocumentoEmpleado = dto.DocumentoEmpleado,
                    FechaEntrega = dto.FechaEntrega,
                    EstadoId = estadoPendiente.EstadoId,
                    FechaCreado = DateTime.Now,
                };

                _context.Entregainsumos.Add(entrega);
                await _context.SaveChangesAsync();

                // Agregar detalles
                foreach (var item in dto.Detalles)
                {
                    var detalle = new DetalleEntrega
                    {
                        EntregainsumoId = entrega.EntregainsumoId,
                        InsumoId = item.InsumoId,
                        Cantidad = item.Cantidad
                    };
                    _context.DetalleEntregas.Add(detalle);
                }

                // Reducir stock de insumos (ya validados)
                foreach (var item in dto.Detalles)
                {
                    var insumo = await _context.Insumos.FindAsync(item.InsumoId);
                    insumo.Stock -= item.Cantidad;
                    // opcional: proteger contra stock negativo por concurrencia
                    if (insumo.Stock < 0)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { success = false, message = $"Operaci�n inv�lida: stock insuficiente al intentar disminuir Insumo {item.InsumoId}" });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Recargar entrega con el estado para la respuesta
                await _context.Entry(entrega).Reference(e => e.Estado).LoadAsync();

                // Construir respuesta
                var response = new EntregaResponseDto
                {
                    EntregainsumoId = entrega.EntregainsumoId,
                    UsuarioId = entrega.UsuarioId,
                    DocumentoEmpleado = entrega.DocumentoEmpleado,
                    FechaCreado = entrega.FechaCreado ?? DateTime.MinValue,
                    FechaEntrega = entrega.FechaEntrega,
                    FechaCompletado = entrega.FechaCompletado,
                    EstadoId = entrega.EstadoId,
                    Estado = ObtenerNombreEstado(entrega),
                    Detalles = dto.Detalles,
                };

                return CreatedAtAction(nameof(BuscarEntrega), new { id = entrega.EntregainsumoId }, response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al crear entrega: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ACTUALIZAR ENTREGA
        // ============================================================
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Entregas")]
        public async Task<ActionResult<EntregaResponseDto>> ActualizarEntrega(int id, [FromBody] ActualizarEntregaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entrega = await _context.Entregainsumos
                    .Include(e => e.DetalleEntregas)
                    .FirstOrDefaultAsync(e => e.EntregainsumoId == id);

                if (entrega == null)
                    return NotFound(new { success = false, message = "Entrega no encontrada" });

                // Determinar si se permite editar: no permitido si ya est� Cancelado o Completado
                var estadoActual = ObtenerNombreEstado(entrega);
                if (estadoActual == "Cancelado" || estadoActual == "Completado")
                {
                    return BadRequest(new { success = false, message = $"No se puede editar una entrega con estado '{estadoActual}'" });
                }

                // Aplicar cambios b�sicos
                if (dto.UsuarioId.HasValue)
                    entrega.UsuarioId = dto.UsuarioId.Value;

                if (!string.IsNullOrWhiteSpace(dto.DocumentoEmpleado))
                {
                    var empleadoExiste = await _context.Empleados.AnyAsync(e => e.DocumentoEmpleado == dto.DocumentoEmpleado);
                    if (!empleadoExiste)
                        return BadRequest(new { success = false, message = "Empleado no encontrado" });

                    entrega.DocumentoEmpleado = dto.DocumentoEmpleado;
                }

                if (dto.FechaEntrega.HasValue)
                    entrega.FechaEntrega = dto.FechaEntrega.Value;

                // Reemplazar detalles si se env�an
                if (dto.Detalles != null)
                {
                    // Validar detalles (valores)
                    foreach (var item in dto.Detalles)
                    {
                        if (item.Cantidad <= 0)
                            return BadRequest(new { success = false, message = "Las cantidades deben ser mayores a 0" });

                        var insumoCheck = await _context.Insumos.FindAsync(item.InsumoId);
                        if (insumoCheck == null)
                            return BadRequest(new { success = false, message = $"Insumo {item.InsumoId} no encontrado" });
                    }

                    // Restaurar stock de previos (las entregas pendientes ya habr�an reducido stock al crear)
                    var previos = entrega.DetalleEntregas?.ToList() ?? new List<DetalleEntrega>();
                    foreach (var prev in previos)
                    {
                        var insPrev = await _context.Insumos.FindAsync(prev.InsumoId);
                        if (insPrev != null)
                        {
                            insPrev.Stock += prev.Cantidad;
                        }
                    }

                    // Eliminar previos
                    if (previos.Any())
                        _context.DetalleEntregas.RemoveRange(previos);

                    await _context.SaveChangesAsync();

                    // Validar stock disponible para nuevos detalles (despu�s de restaurar)
                    foreach (var item in dto.Detalles)
                    {
                        var insumo = await _context.Insumos.FindAsync(item.InsumoId);
                        if (insumo == null)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { success = false, message = $"Insumo {item.InsumoId} no encontrado" });
                        }

                        if (insumo.Stock < item.Cantidad)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { success = false, message = $"Stock insuficiente para Insumo {item.InsumoId}. Disponible: {insumo.Stock}, requerido: {item.Cantidad}" });
                        }
                    }

                    // Agregar nuevos y reducir stock
                    foreach (var item in dto.Detalles)
                    {
                        _context.DetalleEntregas.Add(new DetalleEntrega
                        {
                            EntregainsumoId = entrega.EntregainsumoId,
                            InsumoId = item.InsumoId,
                            Cantidad = item.Cantidad
                        });

                        var ins = await _context.Insumos.FindAsync(item.InsumoId);
                        ins.Stock -= item.Cantidad;
                        if (ins.Stock < 0)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { success = false, message = $"Operaci�n inv�lida: stock insuficiente al asignar Insumo {item.InsumoId}" });
                        }
                    }
                }

                // Manejo de cambio de estado solicitado por ID
                if (dto.EstadoId.HasValue)
                {
                    var estadoNuevo = await _context.Estados.FindAsync(dto.EstadoId.Value);

                    if (estadoNuevo == null)
                    {
                        return BadRequest(new { success = false, message = $"Estado con ID {dto.EstadoId} no encontrado." });
                    }

                    var estadoNombre = estadoNuevo.Nombre ?? string.Empty;

                    if (estadoNombre.Equals("Completado", StringComparison.OrdinalIgnoreCase))
                    {
                        entrega.FechaCompletado = DateTime.Now;
                        entrega.EstadoId = estadoNuevo.EstadoId;
                        // Ya se descontó stock al crear/reemplazar detalles; no hacer nada adicional.
                    }
                    else if (estadoNombre.Equals("Cancelado", StringComparison.OrdinalIgnoreCase))
                    {
                        entrega.FechaCompletado = null;
                        entrega.EstadoId = estadoNuevo.EstadoId;

                        // Devolver stock de los detalles actuales
                        var actuales = await _context.DetalleEntregas
                            .Where(d => d.EntregainsumoId == entrega.EntregainsumoId)
                            .ToListAsync();

                        foreach (var d in actuales)
                        {
                            var ins = await _context.Insumos.FindAsync(d.InsumoId);
                            if (ins != null)
                            {
                                ins.Stock += d.Cantidad;
                            }
                        }
                    }
                    else if (estadoNombre.Equals("Pendiente", StringComparison.OrdinalIgnoreCase))
                    {
                        entrega.FechaCompletado = null;
                        entrega.EstadoId = estadoNuevo.EstadoId;
                    }
                    else
                    {
                        // Si es otro estado, simplemente lo asignamos
                        entrega.EstadoId = estadoNuevo.EstadoId;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Recargar el estado para la respuesta
                await _context.Entry(entrega).Reference(e => e.Estado).LoadAsync();

                // Construir respuesta actualizada
                var detallesActualizados = await _context.DetalleEntregas
                    .Where(d => d.EntregainsumoId == entrega.EntregainsumoId)
                    .Select(d => new DetalleEntregaItemDto { InsumoId = d.InsumoId, Cantidad = d.Cantidad })
                    .ToListAsync();

                var response = new EntregaResponseDto
                {
                    EntregainsumoId = entrega.EntregainsumoId,
                    UsuarioId = entrega.UsuarioId,
                    DocumentoEmpleado = entrega.DocumentoEmpleado,
                    FechaCreado = entrega.FechaCreado ?? DateTime.MinValue,
                    FechaEntrega = entrega.FechaEntrega,
                    FechaCompletado = entrega.FechaCompletado,
                    EstadoId = entrega.EstadoId,
                    Estado = ObtenerNombreEstado(entrega),
                    Detalles = detallesActualizados,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar entrega: {ex.Message}"
                });
            }
        }
    }
}