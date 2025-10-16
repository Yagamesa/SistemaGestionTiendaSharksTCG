using System;
using System.Collections.Generic;

namespace ProyectoMDGSharksWeb.Models
{
    public class InformeViewModel
    {
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }           // Total de dinero recaudado
        public int TotalProductosVendidos { get; set; }
        public int TotalClientes { get; set; }
        public string ProductoMasVendido { get; set; }
        public string ProductoMasRentable { get; set; }
        public string ClienteTop { get; set; }
        public List<VentasMensualesDTO> VentasMensuales { get; set; } = new();

        // Propiedades para filtro de fechas
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        // ✅ NUEVA propiedad para mostrar detalle de ventas dentro del rango de fecha
        public List<VentaDetalleDTO> VentasFiltradas { get; set; } = new();
    }

    public class VentasMensualesDTO
    {
        public string Mes { get; set; }
        public decimal Total { get; set; }
    }

    public class VentaDetalleDTO
    {
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; }
        public decimal Total { get; set; }
        public string Productos { get; set; } // Ejemplo: "Producto1 x2, Producto2 x1"
    }
}
