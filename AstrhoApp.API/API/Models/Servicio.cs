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

    public bool Estado { get; set; }

    public string? Imagen { get; set; }

    public virtual ICollection<ServicioAgendum> ServicioAgenda { get; set; } = new List<ServicioAgendum>();

    public virtual ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
}
