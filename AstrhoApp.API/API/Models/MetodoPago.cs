using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class MetodoPago
{
    public int MetodopagoId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Agendum> Agenda { get; set; } = new List<Agendum>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
