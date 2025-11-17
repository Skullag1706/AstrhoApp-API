namespace AstrhoApp.API.DTOs
{
    public class CrearPedidoDto
    {
        public int DocumentoCliente { get; set; }
        public string MetodoPago { get; set; } = null!;
        public decimal PorcentajeDescuento { get; set; }
        public List<DetallePedidoDto> Detalles { get; set; } = new();
    }

    public class DetallePedidoDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class PedidoResponseDto
    {
        public string VentaId { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = null!;
    }
}