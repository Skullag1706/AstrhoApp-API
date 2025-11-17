using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Servicio
{
    public int ServicioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Duracion { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<Imagen> Imagens { get; set; } = new List<Imagen>();

    public virtual ICollection<ServicioAgendum> ServicioAgenda { get; set; } = new List<ServicioAgendum>();
}
