using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AstrhoApp.API.Services
{
    public class UserService
    {
        private readonly AstrhoAppDbContext _db;
        private readonly EmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(AstrhoAppDbContext db, EmailService emailService, ILogger<UserService> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        // Genera contraseña temporal segura
        private string GenerateTemporaryPassword(int length = 12)
        {
            const string allowed = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@$?_-";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowed[bytes[i] % allowed.Length];
            }
            return new string(chars);
        }

        // Crea usuario con contraseña temporal, marca MustChangePassword = true y envía email
        public async Task<string> CreateUserWithTemporaryPasswordAsync(CreateUserDto dto)
        {
            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Ya existe un usuario con ese correo.");

            var tempPassword = GenerateTemporaryPassword();

            var user = new Usuario
            {
                Email = dto.Email,
                RolId = dto.RolId,
                Estado = true,
                MustChangePassword = true
            };

            // Hash con BCrypt para compatibilidad con AuthController
            user.Contrasena = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            _db.Usuarios.Add(user);
            await _db.SaveChangesAsync();

            try
            {
                await _emailService.EnviarPasswordTemporal(dto.Email, tempPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo al crear usuario {Email}", dto.Email);
            }

            return tempPassword; // En producción evita devolver la contraseña en la API; aquí es útil para admin/test.
        }

        // Verifica credenciales; devuelve usuario si OK (no se modifica)
        public async Task<Usuario?> VerifyCredentialsAsync(string email, string password)
        {
            var user = await _db.Usuarios.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            if (BCrypt.Net.BCrypt.Verify(password, user.Contrasena))
            {
                return user;
            }
            return null;
        }

        // Cambia la contraseña comprobando la actual (usando BCrypt)
        public async Task<bool> ChangePasswordAsync(int usuarioId, string currentPassword, string newPassword)
        {
            var user = await _db.Usuarios.FindAsync(usuarioId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Contrasena))
                return false;

            user.Contrasena = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.MustChangePassword = false;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}