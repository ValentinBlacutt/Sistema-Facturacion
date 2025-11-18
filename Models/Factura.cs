using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;

namespace sistemaFacturacion.Models 
{
    public class Factura
    {
        [Key] //id como la clave primaria de la tabla
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de la factura es obligatoria.")]
        [DataType(DataType.DateTime)] // Especifica el tipo de dato para la interfaz de usuario
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)] // Formato de visualización
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El total de la factura es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")] 
        [Range(0.00, double.MaxValue, ErrorMessage = "El total no puede ser negativo.")] // Asegura un total no negativo
        public decimal Total { get; set; }
        public ICollection<DetalleFactura> Detalles { get; set; }

        public Factura()
        {
            Detalles = new List<DetalleFactura>();
        }
    }   
}
