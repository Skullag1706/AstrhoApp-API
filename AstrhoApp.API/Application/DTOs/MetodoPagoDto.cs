using System.ComponentModel.DataAnnotations;

namespace AstrhoApp.API.DTOs
{
    public class MetodoPagoDto
    {
        public int MetodopagoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CrearMetodoPagoDto
    {
        [Required]
        [MaxLength(20)]
        public string Nombre { get; set; } = string.Empty;
    }

    public class ActualizarMetodoPagoDto
    {
        [Required]
        [MaxLength(20)]
        public string Nombre { get; set; } = string.Empty;
    }
}