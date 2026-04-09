using AstrhoApp.API.Data;
using AstrhoApp.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AstrhoApp.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerarTokenRecuperacion(Usuario usuario, string codigo)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim("purpose", "password_reset"),
                new Claim("code_hash", BCrypt.Net.BCrypt.HashPassword(codigo)),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64
                )
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:RecoverySecretKey"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:RecoveryAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerarTokenResetFinal(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim("purpose", "password_reset_final"),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64
                )
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:ResetFinalSecretKey"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:ResetFinalAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Se añade parámetro opcional de permisos (nombres)
        public string GenerarTokenLogin(Usuario usuario, IEnumerable<string>? permisos = null)
        {
            var claims = new List<Claim>
            {
                // IDENTIDAD DEL USUARIO
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),

                new Claim(ClaimTypes.Email, usuario.Email),
            };

            // Role claim (si existe)
            if (!string.IsNullOrWhiteSpace(usuario.Rol?.Nombre))
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Rol.Nombre));
            }

            // Añadir claims de permisos (uno por permiso)
            if (permisos != null)
            {
                foreach (var permiso in permisos.Distinct())
                {
                    if (!string.IsNullOrWhiteSpace(permiso))
                    {
                        claims.Add(new Claim("permission", permiso));
                    }
                }
            }

            // iat
            claims.Add(new Claim(
                JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64
            ));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
