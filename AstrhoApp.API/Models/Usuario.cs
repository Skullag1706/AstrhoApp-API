using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string? Email { get; set; }

    public string Contrasena { get; set; } = null!;

    public bool? Estado { get; set; }

    public virtual ICollection<Acceso> Accesos { get; set; } = new List<Acceso>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Entregainsumo> Entregainsumos { get; set; } = new List<Entregainsumo>();

    public virtual Rol Rol { get; set; } = null!;
}
