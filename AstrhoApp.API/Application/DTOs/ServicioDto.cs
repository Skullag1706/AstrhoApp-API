using Microsoft.AspNetCore.Http;
namespace AstrhoApp.API.DTOs
{
    // ===== CREAR SERVICIO =====

    public class CrearServicioDto
    {
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Duracion { get; set; }

        public IFormFile? Imagen { get; set; } // ← archivo real
    }


    // ===== ACTUALIZAR SERVICIO =====
    public class ActualizarServicioDto
    {
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Duracion { get; set; }
        public bool Estado { get; set; }

        public IFormFile? Imagen { get; set; }
    }

    // ===== RESPUESTA DE SERVICIO =====
    public class ServicioResponseDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Duracion { get; set; }
        public bool Estado { get; set; }
        public string? Imagen { get; set; }
    }

    // ===== LISTA DE SERVICIOS =====
    public class ServicioListDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Duracion { get; set; }
        public bool Estado { get; set; }
        public string? Imagen { get; set; }
    }
}