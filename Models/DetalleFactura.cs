using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistemaFacturacion.Models
{
    public class DetalleFactura
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que cero.")]
        public decimal PrecioUnitario { get; set; }

        // Campos para el desglose (solo almacenamiento)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Neto { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoIVA { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoImpuestoInterno { get; set; } = 0;


        // Relaciones
        [ForeignKey("Factura")]
        public int FacturaId { get; set; }
        public Factura Factura { get; set; }

        [ForeignKey("Producto")]
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        // Método para actualizar los campos basado en DesglosePrecio
        public void ActualizarDesgloseImpuestos()
        {
            if (Producto == null) return;

            var desglose = DesglosePrecio.Calcular(new Producto
            {
                Precio = this.PrecioUnitario,
                AlicuotaIVA = Producto.AlicuotaIVA,
                TieneImpuestoInterno = Producto.TieneImpuestoInterno,
                PorcentajeImpuestoInterno = Producto.PorcentajeImpuestoInterno
            });

            this.Neto = desglose.Neto;
            this.MontoIVA = desglose.IVA;
            this.MontoImpuestoInterno = desglose.ImpuestoInterno;
        }
    }
}