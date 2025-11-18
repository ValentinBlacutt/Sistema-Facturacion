using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistemaFacturacion.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }

        [Display(Name = "Código de Barras")]
        [StringLength(20, ErrorMessage = "El código no puede exceder los 20 caracteres.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Solo se permiten números en el código de barras.")]
        public string? CodigoBarras { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Display(Name = "Stock Mínimo de Alerta")]
        [Range(0, int.MaxValue, ErrorMessage = "El valor debe ser positivo.")]
        public int AlertaStockMinimo { get; set; } = 5;

        public bool EstaEliminado { get; set; } = false;

        //Variables para el calculo de impuestos
        public decimal AlicuotaIVA { get; set; } = 0.21m; // 21% por defecto
        public bool TieneImpuestoInterno { get; set; } = false;
        public decimal PorcentajeImpuestoInterno { get; set; } = 0;
    }
}