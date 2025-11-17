using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Imagen
{
    public int ImagenId { get; set; }

    public int? ProductoId { get; set; }

    public int? ServicioId { get; set; }

    public string Url { get; set; } = null!;

    public bool? Principal { get; set; }

    public virtual Producto? Producto { get; set; }

    public virtual Servicio? Servicio { get; set; }
}
