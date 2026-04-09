using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AstrhoApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InsumoController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public InsumoController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // GET: api/Insumo
        [HttpGet]
        [Authorize(Policy = "perm:Insumo")]
        public async Task<ActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Insumos
                    .Include(i => i.Categoria)
                    .AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(i => 
                        i.Nombre.ToLower().Contains(busqueda) || 
                        i.Sku.ToLower().Contains(busqueda) ||
                        (i.Descripcion != null && i.Descripcion.ToLower().Contains(busqueda)) ||
                        (i.Categoria != null && i.Categoria.Nombre.ToLower().Contains(busqueda))
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var list = await query
                    .OrderBy(i => i.Nombre)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(i => new InsumoDto
                    {
                        InsumoId = i.InsumoId,
                        Sku = i.Sku,
                        Nombre = i.Nombre,
                        Descripcion = i.Descripcion,
                        CategoriaId = i.CategoriaId,
                        CategoriaNombre = i.Categoria.Nombre,
                        Estado = i.Estado,
                        Stock = i.Stock
                    })
                    .ToListAsync();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = list 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al obtener insumos: {ex.Message}" });
            }
        }

        // GET: api/Insumo/5
        [HttpGet("{id:int}")]
        [Authorize(Policy = "perm:Insumo")]
        public async Task<ActionResult<InsumoDto>> GetById(int id)
        {
            var insumo = await _context.Insumos
                .AsNoTracking()
                .Where(i => i.InsumoId == id)
                .Select(i => new InsumoDto
                {
                    InsumoId = i.InsumoId,
                    Sku = i.Sku,
                    Nombre = i.Nombre,
                    Descripcion = i.Descripcion,
                    CategoriaId = i.CategoriaId,
                    CategoriaNombre = i.Categoria.Nombre,
                    Estado = i.Estado,
                    Stock = i.Stock
                })
                .FirstOrDefaultAsync();

            if (insumo is null) return NotFound();

            return Ok(insumo);
        }

        // POST: api/Insumo
        [HttpPost]
        [Authorize(Policy = "perm:Insumo")]
        public async Task<ActionResult<InsumoDto>> Create([FromBody] CrearInsumoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Generar SKU autom�ticamente a partir del nombre (3 letras + '-' + 3 n�meros)
            string sku;
            try
            {
                sku = await GenerateUniqueSkuAsync(dto.Nombre);
            }
            catch (Exception ex)
            {
                // Si por alguna raz�n no se puede generar un SKU �nico
                return StatusCode(500, new { message = "No se pudo generar un SKU �nico.", detail = ex.Message });
            }

            var entity = new Insumo
            {
                Sku = sku,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CategoriaId = dto.CategoriaId,
                Estado = dto.Estado,
                Stock = dto.Stock // <-- se agrega el stock al crear insumo
            };

            _context.Insumos.Add(entity);
            await _context.SaveChangesAsync();

            // Obtener datos relacionados para DTO
            await _context.Entry(entity).Reference(e => e.Categoria).LoadAsync();
            var result = new InsumoDto
            {
                InsumoId = entity.InsumoId,
                Sku = entity.Sku,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                CategoriaId = entity.CategoriaId,
                CategoriaNombre = entity.Categoria?.Nombre ?? string.Empty,
                Estado = entity.Estado,
                Stock = entity.Stock
            };

            return CreatedAtAction(nameof(GetById), new { id = entity.InsumoId }, result);
        }

        // PUT: api/Insumo/5
        [HttpPut("{id:int}")]
        [Authorize(Policy = "perm:Insumo")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarInsumoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await _context.Insumos.FindAsync(id);
            if (entity is null) return NotFound();

            // Validar SKU �nico si se modific�
            if (!string.Equals(entity.Sku, dto.Sku, StringComparison.OrdinalIgnoreCase)
                && await _context.Insumos.AnyAsync(x => x.Sku == dto.Sku && x.InsumoId != id))
            {
                return Conflict(new { message = "El SKU ya existe." });
            }

            entity.Sku = dto.Sku;
            entity.Nombre = dto.Nombre;
            entity.Descripcion = dto.Descripcion;
            entity.CategoriaId = dto.CategoriaId;
            entity.Estado = dto.Estado;
            entity.Stock = dto.Stock; // <-- actualizar stock en PUT

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Insumo/5
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "perm:Insumo")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Insumos.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Insumos.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- Helpers ---
        private async Task<string> GenerateUniqueSkuAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido para generar el SKU.", nameof(nombre));

            // Normalizar y limpiar el nombre (quitar acentos y caracteres no alfanum�ricos)
            string cleaned = RemoveDiacritics(nombre);
            cleaned = Regex.Replace(cleaned, @"[^A-Za-z0-9]", string.Empty).ToUpperInvariant();

            // Tomar primeras 3 letras (si no hay suficientes, rellenar con 'X')
            string prefix = cleaned.Length >= 3 ? cleaned.Substring(0, 3) : cleaned.PadRight(3, 'X');

            // Intentar generar un SKU �nico
            const int maxAttempts = 1000;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int number = Random.Shared.Next(0, 1000);
                string skuCandidate = $"{prefix}-{number:000}";

                bool exists = await _context.Insumos.AnyAsync(i => i.Sku == skuCandidate);
                if (!exists) return skuCandidate;
            }

            throw new InvalidOperationException("No se pudo generar un SKU �nico tras m�ltiples intentos.");
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}