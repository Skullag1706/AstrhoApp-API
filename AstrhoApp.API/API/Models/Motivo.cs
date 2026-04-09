
using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Motivo
{
    public int MotivoId { get; set; }
    public string Descripcion { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string DocumentoEmpleado { get; set; }
    public int EstadoId { get; set; }
    public virtual Empleado Empleado { get; set; }
    public virtual Estado Estado { get; set; }
}
