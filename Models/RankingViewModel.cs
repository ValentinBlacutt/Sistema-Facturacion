using System.Collections.Generic;

namespace sistemaFacturacion.Models
{
    public class RankingViewModel
    {
        public List<ProductoVendido> MasVendidos { get; set; }
        public List<ProductoVendido> MenosVendidos { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        
        public decimal TotalFacturado { get; set; }
        public int CantidadTotalProductos { get; set; }
    }

    public class ProductoVendido
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
    }
}

