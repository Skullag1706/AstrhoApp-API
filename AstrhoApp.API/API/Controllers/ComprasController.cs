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
    public class ComprasController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public ComprasController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // GET: api/Compras
        [HttpGet]
        [Authorize(Policy = "perm:Compras")]
        public async Task<IActionResult> ObtenerCompras([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Compras
                    .Include(c => c.Proveedor)
                    .Include(c => c.DetalleCompras)
                        .ThenInclude(dc => dc.Insumo)
                    .AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();

                    // Intentar parsear el total para búsqueda numérica si es posible
                    decimal totalBusqueda = 0;
                    bool esNumero = decimal.TryParse(busqueda, out totalBusqueda);

                    query = query.Where(c => 
                        c.CompraId.ToString().Contains(busqueda) || 
                        (c.Proveedor != null && c.Proveedor.Nombre.ToLower().Contains(busqueda)) ||
                        (esNumero && c.Total == totalBusqueda)
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var compras = await query
                    .OrderByDescending(c => c.FechaRegistro)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToListAsync();

                var resultados = compras.Select(c => new CompraResponseDto
                {
                    CompraId = c.CompraId,
                    FechaRegistro = c.FechaRegistro ?? DateTime.MinValue,
                    ProveedorId = c.ProveedorId,
                    ProveedorNombre = c.Proveedor?.Nombre ?? string.Empty,
                    Iva = c.Iva ?? 0m,
                    Subtotal = c.Subtotal,
                    Total = c.Total,
                    Observacion = c.Observacion,
                    Estado = c.Estado ?? false,
                    Detalles = c.DetalleCompras.Select(dc => new DetalleCompraResponseDto
                    {
                        DetalleCompraId = dc.DetalleCompraId,
                        InsumoId = dc.InsumoId,
                        InsumoNombre = dc.Insumo?.Nombre ?? string.Empty,
                        Cantidad = dc.Cantidad,
                        PrecioUnitario = dc.PrecioUnitario,
                        Subtotal = dc.PrecioUnitario * dc.Cantidad
                    }).ToList()
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
                return StatusCode(500, new { success = false, message = $"Error al obtener compras: {ex.Message}" });
            }
        }

        // GET: api/Compras/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "perm:Compras")]
        public async Task<IActionResult> ObtenerCompra(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.DetalleCompras)
                    .ThenInclude(dc => dc.Insumo)
                .FirstOrDefaultAsync(c => c.CompraId == id);

            if (compra == null)
                return NotFound(new { success = false, message = "Compra no encontrada" });

            var dto = new CompraResponseDto
            {
                CompraId = compra.CompraId,
                FechaRegistro = compra.FechaRegistro ?? DateTime.MinValue,
                ProveedorId = compra.ProveedorId,
                ProveedorNombre = compra.Proveedor?.Nombre ?? string.Empty,
                Iva = compra.Iva ?? 0m,
                Subtotal = compra.Subtotal,
                Total = compra.Total,
                Observacion = compra.Observacion,
                Estado = compra.Estado ?? false,
                Detalles = compra.DetalleCompras.Select(dc => new DetalleCompraResponseDto
                {
                    DetalleCompraId = dc.DetalleCompraId,
                    InsumoId = dc.InsumoId,
                    InsumoNombre = dc.Insumo?.Nombre ?? string.Empty,
                    Cantidad = dc.Cantidad,
                    PrecioUnitario = dc.PrecioUnitario,
                    Subtotal = dc.PrecioUnitario * dc.Cantidad
                }).ToList()
            };

            return Ok(new { success = true, data = dto });
        }

        // POST: api/Compras
        [HttpPost]
        [Authorize(Policy = "perm:Compras")]
        public async Task<IActionResult> CrearCompra([FromBody] CrearCompraDto dto)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
                return BadRequest(new { success = false, message = "Debe proporcionar items de la compra" });

            // Validar proveedor
            var proveedor = await _context.Proveedors.FindAsync(dto.ProveedorId);
            if (proveedor == null)
                return BadRequest(new { success = false, message = "Proveedor no encontrado" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar insumos y calcular subtotal
                decimal subtotal = 0m;
                var insumosCache = new Dictionary<int, Insumo>();

                foreach (var item in dto.Items)
                {
                    if (item.Cantidad <= 0)
                        return BadRequest(new { success = false, message = $"Cantidad inv�lida para insumo {item.InsumoId}" });

                    var insumo = await _context.Insumos.FindAsync(item.InsumoId);
                    if (insumo == null)
                        return BadRequest(new { success = false, message = $"Insumo {item.InsumoId} no encontrado" });

                    insumosCache[item.InsumoId] = insumo;
                    subtotal += item.PrecioUnitario * item.Cantidad;
                }

                var ivaPercent = dto.Iva ?? 19m;
                var total = subtotal + (subtotal * ivaPercent / 100m);

                var compra = new Compra
                {
                    ProveedorId = dto.ProveedorId,
                    Iva = ivaPercent,
                    Subtotal = subtotal,
                    Total = total,
                    Estado = true,
                    Observacion = dto.Observacion,
                    FechaRegistro = DateTime.Now
                };

                await _context.Compras.AddAsync(compra);
                await _context.SaveChangesAsync();

                // Crear detalles y actualizar stock/precio compra
                foreach (var item in dto.Items)
                {
                    var detalle = new DetalleCompra
                    {
                        CompraId = compra.CompraId,
                        InsumoId = item.InsumoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario
                    };
                    await _context.DetalleCompras.AddAsync(detalle);

                    // Actualizar stock del insumo: sumar la cantidad comprada
                    var insumoEntity = insumosCache[item.InsumoId];
                    insumoEntity.Stock += item.Cantidad;
                    _context.Insumos.Update(insumoEntity);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Compra registrada correctamente", compraId = compra.CompraId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error al crear compra: {ex.Message}" });
            }
        }

        // PUT: api/Compras/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Compras")]
        public async Task<IActionResult> ActualizarCompra(int id, [FromBody] ActualizarCompraDto dto)
        {
            var compra = await _context.Compras.FindAsync(id);
            if (compra == null)
                return NotFound(new { success = false, message = "Compra no encontrada" });

            try
            {
                // Si se va a cambiar el estado y es una anulaci�n (true -> false), necesitamos ajustar stock
                var estadoAnterior = compra.Estado ?? false;
                var cambiarEstadoA = dto.Estado ?? estadoAnterior;

                if (dto.ProveedorId.HasValue)
                {
                    var prov = await _context.Proveedors.FindAsync(dto.ProveedorId.Value);
                    if (prov == null)
                        return BadRequest(new { success = false, message = "Proveedor no encontrado" });
                    compra.ProveedorId = dto.ProveedorId.Value;
                }

                if (dto.Iva.HasValue)
                    compra.Iva = dto.Iva.Value;

                if (dto.Observacion != null)
                    compra.Observacion = dto.Observacion;

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Si se anula la compra (estado pasa de true a false), restar stock de los insumos
                    if (estadoAnterior && dto.Estado.HasValue && dto.Estado.Value == false)
                    {
                        // Cargar detalles con insumo
                        var detalles = await _context.DetalleCompras
                            .Where(dc => dc.CompraId == compra.CompraId)
                            .Include(dc => dc.Insumo)
                            .ToListAsync();

                        // Validar que haya stock suficiente para restar
                        foreach (var d in detalles)
                        {
                            var insumo = d.Insumo;
                            if (insumo == null)
                                return BadRequest(new { success = false, message = $"Insumo {d.InsumoId} no encontrado para detalle {d.DetalleCompraId}" });

                            if (insumo.Stock < d.Cantidad)
                                return BadRequest(new { success = false, message = $"Stock insuficiente para anular la compra: insumo {insumo.InsumoId} tiene stock {insumo.Stock} pero necesita reducir {d.Cantidad}" });

                            insumo.Stock -= d.Cantidad;
                            _context.Insumos.Update(insumo);
                        }

                        compra.Estado = false;
                    }
                    else
                    {
                        // Si no es una anulaci�n, solo aplicar el estado si viene en el DTO
                        if (dto.Estado.HasValue)
                            compra.Estado = dto.Estado.Value;
                    }

                    _context.Compras.Update(compra);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "Compra actualizada correctamente" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { success = false, message = $"Error al actualizar compra: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al actualizar compra: {ex.Message}" });
            }
        }
    }
}