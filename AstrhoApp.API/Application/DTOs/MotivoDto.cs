namespace AstrhoApp.API.DTOs
{
    public class MotivoCreateDto
    {
        public string Descripcion { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
    }

    public class MotivoResponseDto
    {
        public int MotivoId { get; set; }
        public string Descripcion { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string DocumentoEmpleado { get; set; }
        public int EstadoId { get; set; }
    }
    public class MotivoUpdateDto
    {
        public int EstadoId { get; set; }
    }
}
