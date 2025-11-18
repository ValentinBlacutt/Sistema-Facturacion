namespace sistemaFacturacion.Models
{
    public class DesglosePrecio
    {
        public static DesglosePrecio Calcular(Producto producto)
        {
            decimal porcentajeImpInt = producto.TieneImpuestoInterno ?
                producto.PorcentajeImpuestoInterno / 100m : 0m;

            decimal denominador = 1m + producto.AlicuotaIVA + porcentajeImpInt;
            decimal neto = producto.Precio / denominador;

            return new DesglosePrecio
            {
                Neto = neto,
                IVA = neto * producto.AlicuotaIVA,
                ImpuestoInterno = neto * porcentajeImpInt,
                PrecioFinal = producto.Precio
            };
        }

        public decimal Neto { get; set; }
        public decimal IVA { get; set; }
        public decimal ImpuestoInterno { get; set; }
        public decimal PrecioFinal { get; set; }
    }
}