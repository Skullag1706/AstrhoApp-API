using System.Net;
using System.Net.Mail;
namespace AstrhoApp.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCodigoRecuperacion(string email, string codigo)
        {
            var smtp = new SmtpClient
            {
                Host = _config["Smtp:Host"],
                Port = int.Parse(_config["Smtp:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["Smtp:User"],
                    _config["Smtp:Password"]
                )
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"]),
                Subject = "Código de recuperación de contraseña",
                Body = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='UTF-8'>
</head>
<body style='margin:0; padding:0; font-family: Arial, Helvetica, sans-serif; background-color:#f4f4f7;'>

  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr>
      <td align='center' style='padding:40px 0;'>

        <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 8px 25px rgba(0,0,0,0.1);'>

          <!-- Header con gradiente -->
          <tr>
            <td style='background: linear-gradient(135deg, #ec4899, #8b5cf6); padding:30px; text-align:center; color:#ffffff;'>
              <h1 style='margin:0; font-size:24px;'>Recuperación de contraseña</h1>
            </td>
          </tr>

          <!-- Contenido -->
          <tr>
            <td style='padding:30px; color:#333333;'>
              <p style='font-size:16px;'>Hola 👋,</p>

              <p style='font-size:16px;'>
                Recibimos una solicitud para restablecer tu contraseña.
                Usa el siguiente código para continuar:
              </p>

              <!-- Código -->
              <div style='margin:30px 0; text-align:center;'>
                <span style='display:inline-block; padding:15px 30px; font-size:22px; letter-spacing:4px; font-weight:bold; color:#8b5cf6; background:#f3e8ff; border-radius:8px;'>
                  {codigo}
                </span>
              </div>

              <p style='font-size:14px; color:#555;'>
                ⏱️ Este código expira en <strong>10 minutos</strong>.
              </p>

              <p style='font-size:14px; color:#777;'>
                Si no solicitaste el cambio de contraseña, puedes ignorar este mensaje de forma segura.
              </p>

              <p style='margin-top:30px; font-size:14px;'>
                Atentamente,<br>
                <strong>AstrhoApp</strong>
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='background:#f9fafb; padding:15px; text-align:center; font-size:12px; color:#999;'>
              © {DateTime.Now.Year} AstrhoApp. Todos los derechos reservados.
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>

</body>
</html>
",
                IsBodyHtml = true
            };

            mensaje.To.Add(email);

            await smtp.SendMailAsync(mensaje);
        }

    public async Task EnviarPasswordTemporal(string email, string tempPassword)
        {
            var smtp = new SmtpClient
            {
                Host = _config["Smtp:Host"],
                Port = int.Parse(_config["Smtp:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["Smtp:User"],
                    _config["Smtp:Password"]
                )
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"]),
                Subject = "Acceso a tu cuenta - Contraseña temporal",
                Body = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='UTF-8'>
</head>
<body style='margin:0; padding:0; font-family: Arial, Helvetica, sans-serif; background-color:#f4f4f7;'>

  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr>
      <td align='center' style='padding:40px 0;'>

        <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 8px 25px rgba(0,0,0,0.1);'>

          <!-- Header -->
          <tr>
            <td style='background: linear-gradient(135deg, #6366f1, #8b5cf6); padding:30px; text-align:center; color:#ffffff;'>
              <h1 style='margin:0; font-size:24px;'>Bienvenido a AstrhoApp</h1>
            </td>
          </tr>

          <!-- Contenido -->
          <tr>
            <td style='padding:30px; color:#333333;'>

              <p style='font-size:16px;'>Hola 👋,</p>

              <p style='font-size:16px;'>
                Se ha creado una cuenta para ti en <strong>AstrhoApp</strong>.
                Para acceder por primera vez usa la siguiente contraseña temporal:
              </p>

              <!-- Password -->
              <div style='margin:30px 0; text-align:center;'>
                <span style='display:inline-block; padding:15px 30px; font-size:20px; letter-spacing:3px; font-weight:bold; color:#6366f1; background:#eef2ff; border-radius:8px;'>
                  {tempPassword}
                </span>
              </div>

              <p style='font-size:14px; color:#555;'>
                🔐 Por seguridad, deberás cambiar esta contraseña la primera vez que inicies sesión.
              </p>

              <p style='font-size:14px; color:#777;'>
                Si no esperabas este correo, puedes ignorarlo.
              </p>

              <p style='margin-top:30px; font-size:14px;'>
                Atentamente,<br>
                <strong>AstrhoApp</strong>
              </p>

            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='background:#f9fafb; padding:15px; text-align:center; font-size:12px; color:#999;'>
              © {DateTime.Now.Year} AstrhoApp. Todos los derechos reservados.
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>

</body>
</html>
",
                IsBodyHtml = true
            };

            mensaje.To.Add(email);

            await smtp.SendMailAsync(mensaje);
        }

        public async Task EnviarNotificacionMotivo(string email, string nombreEmpleado, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin, string descripcion)
        {
            var smtp = new SmtpClient
            {
                Host = _config["Smtp:Host"],
                Port = int.Parse(_config["Smtp:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["Smtp:User"],
                    _config["Smtp:Password"]
                )
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"]),
                Subject = "Notificación de Ausencia / Motivo Registrado",
                Body = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='UTF-8'>
</head>
<body style='margin:0; padding:0; font-family: Arial, Helvetica, sans-serif; background-color:#f4f4f7;'>

  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr>
      <td align='center' style='padding:40px 0;'>

        <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 8px 25px rgba(0,0,0,0.1);'>

          <!-- Header -->
          <tr>
            <td style='background: linear-gradient(135deg, #10b981, #3b82f6); padding:30px; text-align:center; color:#ffffff;'>
              <h1 style='margin:0; font-size:24px;'>Nuevo Motivo Registrado</h1>
            </td>
          </tr>

          <!-- Contenido -->
          <tr>
            <td style='padding:30px; color:#333333;'>

              <p style='font-size:16px;'>Hola 👋,</p>

              <p style='font-size:16px;'>
                Se ha registrado un nuevo motivo de ausencia en el sistema:
              </p>

              <!-- Detalles -->
              <div style='margin:20px 0; padding:20px; background:#f9fafb; border-radius:8px; border-left:4px solid #10b981;'>
                <p style='margin:5px 0;'><strong>Empleado:</strong> {nombreEmpleado}</p>
                <p style='margin:5px 0;'><strong>Fecha:</strong> {fecha:dd/MM/yyyy}</p>
                <p style='margin:5px 0;'><strong>Hora Inicio:</strong> {horaInicio:HH:mm}</p>
                <p style='margin:5px 0;'><strong>Hora Fin:</strong> {horaFin:HH:mm}</p>
                <p style='margin:5px 0;'><strong>Descripción:</strong> {descripcion}</p>
              </div>

              <p style='font-size:14px; color:#555;'>
                ⚠️ Las citas programadas para este empleado en este rango de tiempo han sido canceladas automáticamente.
              </p>

              <p style='margin-top:30px; font-size:14px;'>
                Atentamente,<br>
                <strong>AstrhoApp</strong>
              </p>

            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='background:#f9fafb; padding:15px; text-align:center; font-size:12px; color:#999;'>
              © {DateTime.Now.Year} AstrhoApp. Todos los derechos reservados.
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>

</body>
</html>
",
                IsBodyHtml = true
            };

            mensaje.To.Add(email);

            await smtp.SendMailAsync(mensaje);
        }
    }
}
