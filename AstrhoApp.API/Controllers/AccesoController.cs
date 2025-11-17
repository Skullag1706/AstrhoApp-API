using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public AccesoController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LOGIN
        // ============================================================
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.NombreUsuario == loginDto.NombreUsuario
                                           && u.Estado == true);

                if (usuario == null)
                {
                    return Ok(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario no encontrado o inactivo"
                    });
                }

                // Aquí deberías validar la contraseña hasheada
                // Por simplicidad, comparo directamente (NO RECOMENDADO EN PRODUCCIÓN)
                if (usuario.Contrasena != loginDto.Contrasena)
                {
                    return Ok(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Contraseña incorrecta"
                    });
                }

                // Registrar acceso
                var acceso = new Acceso
                {
                    UsuarioId = usuario.UsuarioId
                };
                _context.Accesos.Add(acceso);
                await _context.SaveChangesAsync();

                return Ok(new LoginResponseDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = "TOKEN_JWT_AQUI", // Implementar JWT después
                    Usuario = new UsuarioDto
                    {
                        UsuarioId = usuario.UsuarioId,
                        NombreUsuario = usuario.NombreUsuario,
                        Email = usuario.Email,
                        RolNombre = usuario.Rol.Nombre
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // ============================================================
        // CERRAR SESIÓN
        // ============================================================
        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            try
            {
                // Buscar el usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.UsuarioId == logoutDto.UsuarioId);

                if (usuario == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Usuario no encontrado"
                    });
                }

                // Aquí podrías invalidar el token JWT si estás usando una lista negra
                // O simplemente registrar el cierre de sesión

                return Ok(new
                {
                    success = true,
                    message = "Sesión cerrada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // ============================================================
        // RECUPERAR CONTRASEÑA - ENVIAR CÓDIGO
        // ============================================================
        [HttpPost("recuperar-password")]
        public async Task<ActionResult> RecuperarPassword([FromBody] RecuperarPasswordDto dto)
        {
            try
            {
                // Buscar usuario por email o nombre de usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => (u.Email == dto.Email || u.NombreUsuario == dto.NombreUsuario)
                                           && u.Estado == true);

                if (usuario == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No se encontró un usuario con esos datos"
                    });
                }

                if (string.IsNullOrEmpty(usuario.Email))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "El usuario no tiene un correo electrónico registrado"
                    });
                }

                // Generar código de 6 dígitos
                var codigoRecuperacion = new Random().Next(100000, 999999).ToString();

                // Guardar código y fecha de expiración (puedes crear una tabla temporal o usar caché)
                // Por simplicidad, lo guardamos en una tabla temporal o en memoria
                // NOTA: Deberías crear una tabla RecuperacionPassword con: usuario_id, codigo, fecha_expiracion

                // Aquí enviarías el email con el código
                // await _emailService.EnviarCodigoRecuperacion(usuario.Email, codigoRecuperacion);

                // Por ahora, devolvemos el código (EN PRODUCCIÓN NUNCA HACER ESTO)
                return Ok(new
                {
                    success = true,
                    message = "Se ha enviado un código de recuperación a tu correo electrónico",
                    // REMOVER EN PRODUCCIÓN:
                    codigoGenerado = codigoRecuperacion,
                    email = usuario.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // ============================================================
        // VERIFICAR CÓDIGO DE RECUPERACIÓN
        // ============================================================
        [HttpPost("verificar-codigo")]
        public async Task<ActionResult> VerificarCodigo([FromBody] VerificarCodigoDto dto)
        {
            try
            {
                // Buscar usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Estado == true);

                if (usuario == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Usuario no encontrado"
                    });
                }

                // Aquí verificarías el código desde tu tabla de códigos temporales
                // Por simplicidad, asumimos que es válido
                // var codigoValido = await _context.RecuperacionPassword
                //     .AnyAsync(r => r.UsuarioId == usuario.UsuarioId 
                //                 && r.Codigo == dto.Codigo 
                //                 && r.FechaExpiracion > DateTime.Now);

                // Simulación de validación
                bool codigoValido = !string.IsNullOrEmpty(dto.Codigo);

                if (!codigoValido)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Código inválido o expirado"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Código verificado correctamente",
                    usuarioId = usuario.UsuarioId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // ============================================================
        // CAMBIAR CONTRASEÑA
        // ============================================================
        [HttpPost("cambiar-password")]
        public async Task<ActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.UsuarioId == dto.UsuarioId);

                if (usuario == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Usuario no encontrado"
                    });
                }

                // Validar que la nueva contraseña cumpla requisitos
                if (dto.NuevaPassword.Length < 6)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "La contraseña debe tener al menos 6 caracteres"
                    });
                }

                if (dto.NuevaPassword != dto.ConfirmarPassword)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Las contraseñas no coinciden"
                    });
                }

                // Actualizar contraseña (EN PRODUCCIÓN: hashear la contraseña)
                usuario.Contrasena = dto.NuevaPassword;
                await _context.SaveChangesAsync();

                // Eliminar código de recuperación usado
                // await _context.RecuperacionPassword.Where(r => r.UsuarioId == usuario.UsuarioId).DeleteAsync();

                return Ok(new
                {
                    success = true,
                    message = "Contraseña actualizada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
    }
}