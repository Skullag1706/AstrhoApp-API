namespace AstrhoApp.API.DTOs
{
    // ===== CREAR USUARIO =====
    public class CrearUsuarioDto
    {
        public int RolId { get; set; }
        public string? Email { get; set; }
        public string Contrasena { get; set; } = null!;
        public string ConfirmarContrasena { get; set; } = null!;
    }

    // ===== ACTUALIZAR USUARIO =====
    public class ActualizarUsuarioDto
    {
        public int RolId { get; set; }
        public string? Email { get; set; }
        public string Contrasena { get; set; } = null!;
        public string ConfirmarContrasena { get; set; } = null!;
        public bool Estado { get; set; }
    }

    // ===== RESPUESTA DE USUARIO =====
    public class UsuarioResponseDto
    {
        public int UsuarioId { get; set; }
        public string? Email { get; set; }
        public string Contrasena { get; set; } = null!;
        public bool Estado { get; set; }
        public RolDto Rol { get; set; } = null!;
    }

    public class RolDto
    {
        public int RolId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
    }

    // ===== LISTA DE USUARIOS =====
    public class UsuarioListDto
    {
        public int UsuarioId { get; set; }
        public string? Email { get; set; }
        public string Contrasena { get; set; } = null!;
        public bool Estado { get; set; }
        public string RolNombre { get; set; } = null!;
    }

    public class CreateUserDto
    {
        public string Email { get; set; }
        public int RolId { get; set; }
        // opcional: otros campos que quieras asignar al crear usuario (nombre, etc.)
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}