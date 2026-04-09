using System.ComponentModel.DataAnnotations;

namespace AstrhoApp.API.DTOs
{
    public class InsumoDto
    {
        public int InsumoId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public int Stock { get; set; }
        public bool? Estado { get; set; }
    }

    public class CrearInsumoDto
    {
        [Required]
        [MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        public bool? Estado { get; set; } = true;

        public int Stock { get; set; } = 0;
    }

    public class ActualizarInsumoDto
    {
        [Required]
        [MaxLength(20)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        public bool? Estado { get; set; } = true;

        public int Stock { get; set; } = 0;
    }

    public class StockAdjustDto
    {
        /// <summary>
        /// Cantidad a ajustar. Valor positivo suma, valor negativo resta.
        /// </summary>
        [Required]
        public int Amount { get; set; }
    }

    public class StockSetDto
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}