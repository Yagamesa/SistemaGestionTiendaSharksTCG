namespace ProyectoMDGSharksWeb.Models.ViewModels
{
    public class VentaResumenViewModel
    {
        public Venta Venta { get; set; } = null!;

        public decimal TotalBruto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Pago { get; set; }
        public decimal TotalFinal => TotalBruto - Descuento;
        public decimal Deuda => Math.Max(0, TotalFinal - Pago);
    }
}
