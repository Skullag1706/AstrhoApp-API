using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Cliente
{
    public string DocumentoCliente { get; set; } = null!;

    public int UsuarioId { get; set; }

    public string? TipoDocumento { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public bool Estado { get; set; }

    public string? Dirección { get; set; }

    public virtual ICollection<Agendum> Agenda { get; set; } = new List<Agendum>();

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
