using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Cliente
{
    public int DocumentoCliente { get; set; }

    public int UsuarioId { get; set; }

    public string? TipoDocumento { get; set; }

    public string Documento { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<Agendum> Agenda { get; set; } = new List<Agendum>();

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
