public class SolicitarRecuperacionDto
{
    public string Email { get; set; }
}

public class ResetPasswordDto
{
    public string ResetToken { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; }
    public string ConfirmarContrasena { get; set; }
}
public class ValidarCodigoDto
{
    public string Token { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}
