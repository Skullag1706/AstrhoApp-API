using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Empleado
{
    public string DocumentoEmpleado { get; set; } = null!;

    public string? TipoDocumento { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public int UsuarioId { get; set; }

    public bool Estado { get; set; }

    public string? Dirección { get; set; }

    public virtual ICollection<Agendum> Agenda { get; set; } = new List<Agendum>();

    public virtual ICollection<Entregainsumo> Entregainsumos { get; set; } = new List<Entregainsumo>();

    public virtual ICollection<HorarioEmpleado> HorarioEmpleados { get; set; } = new List<HorarioEmpleado>();

    public virtual Usuario Usuario { get; set; } = null!;
}
