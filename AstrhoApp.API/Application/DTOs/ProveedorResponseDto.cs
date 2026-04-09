using System.ComponentModel.DataAnnotations;

namespace AstrhoApp.API.DTOs
{
    public class ProveedorResponseDto
    {
        public int ProveedorId { get; set; }
        public string TipoProveedor { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? TipoDocumento { get; set; }
        public string? Documento { get; set; }
        public string? PersonaContacto { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Departamento { get; set; }
        public string? Ciudad { get; set; }
        public bool Estado { get; set; }
    }

    public class CrearProveedorDto
    {
        [Required]
        public string TipoProveedor { get; set; } = string.Empty;

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? TipoDocumento { get; set; }

        [MaxLength(15)]
        public string? Documento { get; set; }

        [MaxLength(100)]
        public string? Persona_Contacto { get; set; }

        [EmailAddress]
        [MaxLength(80)]
        public string? Correo { get; set; }

        [MaxLength(15)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        public string? Direccion { get; set; }

        public string? Departamento { get; set; }

        public string? Ciudad { get; set; }

        // Opcional: permite establecer inicialmente el estado (por defecto true si no se provee)
        public bool? Estado { get; set; }
    }

    public class ActualizarProveedorDto
    {
        public string? TipoProveedor { get; set; }
        public string? Nombre { get; set; }
        public string? TipoDocumento { get; set; }

        [MaxLength(15)]
        public string? Documento { get; set; }

        [MaxLength(100)]
        public string? Persona_Contacto { get; set; }

        [EmailAddress]
        [MaxLength(80)]
        public string? Correo { get; set; }

        [MaxLength(15)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        public string? Direccion { get; set; }
        public string? Departamento { get; set; }
        public string? Ciudad { get; set; }
        public bool? Estado { get; set; }
    }
}