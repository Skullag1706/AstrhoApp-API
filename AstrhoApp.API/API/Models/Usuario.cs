using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public string? Email { get; set; }

    // Ahora almacenamos el hash de la contraseña (no el texto plano).
    public string Contrasena { get; set; } = null!;

    // Nueva bandera para forzar cambio en el primer inicio de sesión.
    public bool MustChangePassword { get; set; } = false;

    public bool Estado { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Entregainsumo> Entregainsumos { get; set; } = new List<Entregainsumo>();

    public virtual Rol Rol { get; set; } = null!;
}
