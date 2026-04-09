namespace AstrhoApp.API.DTOs
{
    // ===== CREAR CATEGORÍA =====
    public class CrearCategoriaDto
    {
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
    }

    // ===== ACTUALIZAR CATEGORÍA =====
    public class ActualizarCategoriaDto
    {
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }

    // ===== RESPUESTA DE CATEGORÍA =====
    public class CategoriaResponseDto
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }

    // ===== LISTA DE CATEGORÍAS =====
    public class CategoriaListDto
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
        public int CantidadProductos { get; set; }
    }
}