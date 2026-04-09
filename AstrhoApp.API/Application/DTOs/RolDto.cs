namespace AstrhoApp.API.DTOs
{
    // DTO para crear rol (puede incluir ids de permisos)
    public class CrearRolDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public List<int>? PermisosIds { get; set; } = new();
    }

    // DTO para actualizar rol (incluye estado y permisos)
    public class ActualizarRolDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public List<int>? PermisosIds { get; set; } = new();
        public bool Estado { get; set; }
    }

    // DTO usado como respuesta detallada (incluye permisos)
    public class RolResponseDto
    {
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
        public List<int> PermisosIds { get; set; } = new();
        public List<string> Permisos { get; set; } = new();
    }

    // DTO para listas (ya incluía lista de nombres de permisos)
    public class RolListDto
    {
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public List<string> Permisos { get; set; } = new();
        public bool Estado { get; set; }
    }
}