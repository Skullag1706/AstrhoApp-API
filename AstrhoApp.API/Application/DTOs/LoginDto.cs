namespace AstrhoApp.API.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }

        public int UsuarioId { get; set; }
        public string? Email { get; set; }
        public string? Rol { get; set; }

        public List<string>? Permisos { get; set; }

        public bool MustChangePassword { get; set; }
    }
}
