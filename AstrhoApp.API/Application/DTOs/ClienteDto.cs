namespace AstrhoApp.API.DTOs
{
    // ===== CREAR CLIENTE =====
    public class CrearClienteDto
    {
        public string DocumentoCliente { get; set; } = null!; // VARCHAR(20)
        public int UsuarioId { get; set; }
        public string TipoDocumento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Dirección { get; set; }
    }

    // ===== ACTUALIZAR CLIENTE =====
    public class ActualizarClienteDto
    {
        public string TipoDocumento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Dirección { get; set; }
        public bool Estado { get; set; }
    }

    // ===== RESPUESTA DE CLIENTE =====
    public class ClienteResponseDto
    {
        public string DocumentoCliente { get; set; } = null!;
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string TipoDocumento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Dirección { get; set; }
        public bool Estado { get; set; }
    }

    // ===== LISTA DE CLIENTES =====
    public class ClienteListDto
    {
        public string DocumentoCliente { get; set; } = null!;
        public string TipoDocumento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Dirección { get; set; }
        public bool Estado { get; set; }
    }
}