namespace AstrhoApp.API.DTOs
{
    // ===== CREAR MARCA =====
    public class CrearMarcaDto
    {
        public string Nombre { get; set; } = null!;
    }

    // ===== ACTUALIZAR MARCA =====
    public class ActualizarMarcaDto
    {
        public string Nombre { get; set; } = null!;
    }

    // ===== RESPUESTA DE MARCA =====
    public class MarcaResponseDto
    {
        public int MarcaId { get; set; }
        public string Nombre { get; set; } = null!;
    }

    // ===== LISTA DE MARCAS =====
    public class MarcaListDto
    {
        public int MarcaId { get; set; }
        public string Nombre { get; set; } = null!;
    }
}
