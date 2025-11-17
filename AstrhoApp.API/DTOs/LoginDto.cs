namespace AstrhoApp.API.DTOs
{
    // ===== LOGIN =====
    public class LoginDto
    {
        public string NombreUsuario { get; set; } = null!;
        public string Contrasena { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? Token { get; set; }
        public UsuarioDto? Usuario { get; set; }
    }

    public class UsuarioDto
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string? Email { get; set; }
        public string RolNombre { get; set; } = null!;
    }

    // ===== LOGOUT =====
    public class LogoutDto
    {
        public int UsuarioId { get; set; }
    }

    // ===== RECUPERAR CONTRASEÑA =====
    public class RecuperarPasswordDto
    {
        public string? Email { get; set; }
        public string? NombreUsuario { get; set; }
    }

    public class VerificarCodigoDto
    {
        public string Email { get; set; } = null!;
        public string Codigo { get; set; } = null!;
    }

    public class CambiarPasswordDto
    {
        public int UsuarioId { get; set; }
        public string NuevaPassword { get; set; } = null!;
        public string ConfirmarPassword { get; set; } = null!;
    }
}