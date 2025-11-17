using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Proveedor
{
    public int ProveedorId { get; set; }

    public string TipoProveedor { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? TipoDocumento { get; set; }

    public string? Documento { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
