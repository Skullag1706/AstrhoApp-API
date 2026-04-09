using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Categorium
{
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();
}
