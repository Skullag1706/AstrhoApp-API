namespace AstrhoApp.API.DTOs
{
    public class EstadoResponseDto
    {
        public int EstadoId { get; set; }
        public string Nombre { get; set; } = null!;
    }

    public class CrearEstadoDto
    {
        public string Nombre { get; set; } = null!;
    }

    public class ModificarEstadoDto
    {
        public string Nombre { get; set; } = null!;
    }
}