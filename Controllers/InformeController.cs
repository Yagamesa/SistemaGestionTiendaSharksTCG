using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System;

namespace ProyectoMDGSharksWeb.Controllers
{
    public class InformeController : Controller
    {
        private readonly SharksDbContext _context;

        public InformeController(SharksDbContext context)
        {
            _context = context;
        }

        // ===========================
        // MÉTODOS DEL INFORME PARA LA VISTA
        // ===========================

        /// <summary>
        /// Acción principal que muestra el informe con filtro opcional por fechas
        /// </summary>
        public IActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var modelo = GenerarInforme(fechaDesde, fechaHasta);

            ViewData["FechaDesde"] = fechaDesde?.ToString("yyyy-MM-dd");
            ViewData["FechaHasta"] = fechaHasta?.ToString("yyyy-MM-dd");

            return View(modelo);
        }

        /// <summary>
        /// Método privado que genera el modelo de datos del informe según rango de fechas
        /// </summary>
        private InformeViewModel GenerarInforme(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var ventas = _context.Venta
                .Include(v => v.ProductoVenta)
                    .ThenInclude(pv => pv.IdProductoNavigation)
                .AsQueryable();

            if (fechaDesde.HasValue)
                ventas = ventas.Where(v => v.FechaVenta >= DateOnly.FromDateTime(fechaDesde.Value));

            if (fechaHasta.HasValue)
                ventas = ventas.Where(v => v.FechaVenta <= DateOnly.FromDateTime(fechaHasta.Value));

            var ventasList = ventas.ToList();

            var totalVentas = ventasList.Count;
            var totalIngresos = ventasList.Sum(v => v.ProductoVenta.Sum(pv => pv.PrecioUnitario * pv.Cantidad));
            var totalProductosVendidos = ventasList.Sum(v => v.ProductoVenta.Sum(pv => pv.Cantidad));
            var totalClientes = ventasList.Select(v => v.IdCliente).Distinct().Count();

            var productoMasVendido = ventasList
                .SelectMany(v => v.ProductoVenta)
                .GroupBy(pv => pv.IdProducto)
                .Select(g => new { IdProducto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .OrderByDescending(g => g.Cantidad)
                .FirstOrDefault();

            var productoMasRentable = ventasList
                .SelectMany(v => v.ProductoVenta)
                .GroupBy(pv => pv.IdProducto)
                .Select(g => new
                {
                    IdProducto = g.Key,
                    Ingreso = g.Sum(p => p.Cantidad * p.PrecioUnitario)
                })
                .OrderByDescending(p => p.Ingreso)
                .FirstOrDefault();

            var clienteTop = ventasList
                .GroupBy(v => v.IdCliente)
                .Select(g => new
                {
                    IdCliente = g.Key,
                    Total = g.Sum(v => v.ProductoVenta.Sum(pv => pv.Cantidad * pv.PrecioUnitario))
                })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            var ventasPorMes = ventasList
                .Where(v => v.FechaVenta != null)
                .GroupBy(v => new { v.FechaVenta.Value.Year, v.FechaVenta.Value.Month })
                .Select(g => new VentasMensualesDTO
                {
                    Mes = CultureInfo.GetCultureInfo("es-ES")
                            .DateTimeFormat.GetMonthName(g.Key.Month)
                            .Substring(0, 1).ToUpper() +
                          CultureInfo.GetCultureInfo("es-ES")
                            .DateTimeFormat.GetMonthName(g.Key.Month)
                            .Substring(1) + " " + g.Key.Year,
                    Total = g.Sum(v => v.ProductoVenta.Sum(pv => pv.Cantidad * pv.PrecioUnitario))
                })
                // Ordenar por año y mes para evitar problemas al ordenar por string del mes
                .OrderBy(g => g.Mes)
                .ToList();

            return new InformeViewModel
            {
                TotalVentas = totalVentas,
                TotalIngresos = totalIngresos,
                TotalProductosVendidos = totalProductosVendidos,
                TotalClientes = totalClientes,
                ProductoMasVendido = productoMasVendido?.IdProducto != null
                    ? _context.Productos.Find(productoMasVendido.IdProducto)?.Nombre ?? "N/A"
                    : "N/A",
                ProductoMasRentable = productoMasRentable?.IdProducto != null
                    ? _context.Productos.Find(productoMasRentable.IdProducto)?.Nombre ?? "N/A"
                    : "N/A",
                ClienteTop = clienteTop?.IdCliente != null
    ? (_context.Clientes.Find(clienteTop.IdCliente) is Cliente cliente)
        ? $"{cliente.Nombre} {cliente.ApellidoPaterno} {cliente.ApellidoMaterno}"
        : "N/A"
    : "N/A",

                VentasMensuales = ventasPorMes,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta
            };
        }


        // ===========================
        // MÉTODOS PARA EXPORTACIÓN DE REPORTES (Excel y PDF)
        // ===========================


        private List<VentaDetalleDTO> ObtenerVentasFiltradas(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var ventasQuery = _context.Venta
                .Include(v => v.ProductoVenta)
                    .ThenInclude(pv => pv.IdProductoNavigation)
                .Include(v => v.IdClienteNavigation)
                .AsQueryable();

            if (fechaDesde.HasValue)
                ventasQuery = ventasQuery.Where(v => v.FechaVenta >= DateOnly.FromDateTime(fechaDesde.Value));

            if (fechaHasta.HasValue)
                ventasQuery = ventasQuery.Where(v => v.FechaVenta <= DateOnly.FromDateTime(fechaHasta.Value));

            var ventas = ventasQuery.ToList();

            var listaDetalle = ventas.Select(v => new VentaDetalleDTO
            {
                Fecha = v.FechaVenta.HasValue ? v.FechaVenta.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                Cliente = v.IdClienteNavigation != null
    ? $"{v.IdClienteNavigation.Nombre} {v.IdClienteNavigation.ApellidoPaterno} {v.IdClienteNavigation.ApellidoMaterno}"
    : "N/A",

                Total = v.ProductoVenta.Sum(pv => pv.Cantidad * pv.PrecioUnitario),
                Productos = string.Join(", ", v.ProductoVenta.Select(pv => $"{pv.IdProductoNavigation?.Nombre} x{pv.Cantidad}"))
            }).ToList();

            return listaDetalle;
        }

        /// <summary>
        /// Exporta el informe generado a un archivo Excel (.xlsx)
        /// </summary>
        public async Task<IActionResult> ExportarExcel(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var informe = GenerarInforme(fechaDesde, fechaHasta);
            var ventasFiltradas = ObtenerVentasFiltradas(fechaDesde, fechaHasta);

            using (var workbook = new XLWorkbook())
            {
                // Hoja 1: Resumen del Reporte
                var resumenSheet = workbook.Worksheets.Add("Reporte Resumen");
                resumenSheet.Cell(1, 1).Value = "Resumen del Reporte";
                resumenSheet.Range("A1:B1").Merge().Style.Font.SetBold().Font.FontSize = 16;

                resumenSheet.Cell(3, 1).Value = "Total Ventas";
                resumenSheet.Cell(3, 2).Value = informe.TotalVentas;
                resumenSheet.Cell(4, 1).Value = "Total Ingresos";
                resumenSheet.Cell(4, 2).Value = informe.TotalIngresos;
                resumenSheet.Cell(5, 1).Value = "Total Productos Vendidos";
                resumenSheet.Cell(5, 2).Value = informe.TotalProductosVendidos;
                resumenSheet.Cell(6, 1).Value = "Total Clientes";
                resumenSheet.Cell(6, 2).Value = informe.TotalClientes;
                resumenSheet.Cell(7, 1).Value = "Producto Más Vendido";
                resumenSheet.Cell(7, 2).Value = informe.ProductoMasVendido;
                resumenSheet.Cell(8, 1).Value = "Producto Más Rentable";
                resumenSheet.Cell(8, 2).Value = informe.ProductoMasRentable;
                resumenSheet.Cell(9, 1).Value = "Cliente Top";
                resumenSheet.Cell(9, 2).Value = informe.ClienteTop;

                // Aplicar bordes a resumen principal
                resumenSheet.Range("A3:B9").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                resumenSheet.Range("A3:B9").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Ventas Mensuales
                resumenSheet.Cell(11, 1).Value = "Ventas Mensuales";
                resumenSheet.Range("A11:B11").Merge().Style.Font.SetBold().Font.FontSize = 14;
                resumenSheet.Cell(12, 1).Value = "Mes";
                resumenSheet.Cell(12, 2).Value = "Total";
                int fila = 13;
                foreach (var vm in informe.VentasMensuales)
                {
                    resumenSheet.Cell(fila, 1).Value = vm.Mes;
                    resumenSheet.Cell(fila, 2).Value = vm.Total;
                    fila++;
                }
                // Bordes a ventas mensuales
                resumenSheet.Range($"A12:B{fila - 1}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                resumenSheet.Range($"A12:B{fila - 1}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Hoja 2: Detalle de Ventas con columna enumerada
                var detalleSheet = workbook.Worksheets.Add("Reporte Detalle");
                detalleSheet.Cell(1, 1).Value = "Detalle de Ventas Filtradas";
                detalleSheet.Range("A1:E1").Merge().Style.Font.SetBold().Font.FontSize = 16;

                // Encabezados con columna de número
                detalleSheet.Cell(3, 1).Value = "N°";
                detalleSheet.Cell(3, 2).Value = "Fecha";
                detalleSheet.Cell(3, 3).Value = "Cliente";
                detalleSheet.Cell(3, 4).Value = "Total";
                detalleSheet.Cell(3, 5).Value = "Productos";

                int filaDetalle = 4;
                int contador = 1;
                foreach (var venta in ventasFiltradas)
                {
                    detalleSheet.Cell(filaDetalle, 1).Value = contador++;
                    detalleSheet.Cell(filaDetalle, 2).Value = venta.Fecha.ToString("yyyy-MM-dd");
                    detalleSheet.Cell(filaDetalle, 3).Value = venta.Cliente;
                    detalleSheet.Cell(filaDetalle, 4).Value = venta.Total;
                    detalleSheet.Cell(filaDetalle, 5).Value = venta.Productos;
                    filaDetalle++;
                }
                // Bordes a tabla detalle
                detalleSheet.Range($"A3:E{filaDetalle - 1}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                detalleSheet.Range($"A3:E{filaDetalle - 1}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ReporteVentas.xlsx");
                }
            }
        }


        /// <summary>
        /// Exporta el informe generado a un archivo PDF
        /// </summary>
        public async Task<IActionResult> ExportarPdf(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var informe = GenerarInforme(fechaDesde, fechaHasta);
            var ventasFiltradas = ObtenerVentasFiltradas(fechaDesde, fechaHasta);

            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Título
                var titulo = new Paragraph("Reporte de Ventas")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetFont(boldFont);
                document.Add(titulo);

                // Fechas filtro
                document.Add(new Paragraph(
                    $"Desde: {(fechaDesde.HasValue ? fechaDesde.Value.ToString("yyyy-MM-dd") : "Inicio")} " +
                    $"Hasta: {(fechaHasta.HasValue ? fechaHasta.Value.ToString("yyyy-MM-dd") : "Fin")}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20)
                    .SetFont(normalFont));

                // Tabla resumen
                var tablaResumen = new Table(2).UseAllAvailableWidth();
                tablaResumen.AddHeaderCell(new Cell(1, 2).Add(new Paragraph("Resumen")).SetFont(boldFont).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER));

                tablaResumen.AddCell("Total Ventas");
                tablaResumen.AddCell(informe.TotalVentas.ToString());

                tablaResumen.AddCell("Total Ingresos");
                tablaResumen.AddCell(informe.TotalIngresos.ToString("C"));

                tablaResumen.AddCell("Total Productos Vendidos");
                tablaResumen.AddCell(informe.TotalProductosVendidos.ToString());

                tablaResumen.AddCell("Total Clientes");
                tablaResumen.AddCell(informe.TotalClientes.ToString());

                tablaResumen.AddCell("Producto Más Vendido");
                tablaResumen.AddCell(informe.ProductoMasVendido);

                tablaResumen.AddCell("Producto Más Rentable");
                tablaResumen.AddCell(informe.ProductoMasRentable);

                tablaResumen.AddCell("Cliente Top");
                tablaResumen.AddCell(informe.ClienteTop);

                document.Add(tablaResumen);

                // Espacio
                document.Add(new Paragraph("\n"));

                // Ventas mensuales
                var subtituloMensual = new Paragraph("Ventas Mensuales")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetMarginBottom(10);
                document.Add(subtituloMensual);

                var tablaMensual = new Table(2).UseAllAvailableWidth();
                tablaMensual.AddHeaderCell("Mes");
                tablaMensual.AddHeaderCell("Total");

                foreach (var vm in informe.VentasMensuales)
                {
                    tablaMensual.AddCell(vm.Mes);
                    tablaMensual.AddCell(vm.Total.ToString("C"));
                }

                document.Add(tablaMensual);

                // Espacio
                document.Add(new Paragraph("\n"));

                // Detalle de ventas filtradas con numeración
                var subtituloDetalle = new Paragraph("Detalle de Ventas Filtradas")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetMarginBottom(10);
                document.Add(subtituloDetalle);

                // Tabla detalle con 5 columnas (incluye columna número)
                var tablaDetalle = new Table(5).UseAllAvailableWidth();

                // Agregar encabezados con columna para número
                tablaDetalle.AddHeaderCell("N°");
                tablaDetalle.AddHeaderCell("Fecha");
                tablaDetalle.AddHeaderCell("Cliente");
                tablaDetalle.AddHeaderCell("Total");
                tablaDetalle.AddHeaderCell("Productos");

                int contador = 1;
                foreach (var venta in ventasFiltradas)
                {
                    tablaDetalle.AddCell(contador.ToString());
                    tablaDetalle.AddCell(venta.Fecha.ToString("yyyy-MM-dd"));
                    tablaDetalle.AddCell(venta.Cliente);
                    tablaDetalle.AddCell(venta.Total.ToString("C"));
                    tablaDetalle.AddCell(venta.Productos);
                    contador++;
                }

                document.Add(tablaDetalle);

                document.Close();

                var bytes = ms.ToArray();
                return File(bytes, "application/pdf", "ReporteVentas.pdf");
            }
        }

}
}
