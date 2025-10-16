using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Globalization;
using System.Linq;
using System;
using System.IO;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class InformeTorneosController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InformeTorneosController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool TienePermiso(string modulo, string accion)
        {
            var userRol = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (userRol == "Administrador") return true;

            var rol = _context.Rols.FirstOrDefault(r => r.Nombre == userRol);
            if (rol == null) return false;

            return _context.Permisos.Any(p =>
                p.IdRol == rol.Id &&
                p.Modulo == modulo &&
                p.Accion == accion &&
                p.Permitido);
        }

        /// <summary>
        /// Muestra la vista con el informe de torneos
        /// </summary>
        public IActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (!TienePermiso("InformeTorneos", "Ver"))
                return Forbid();

            var modelo = GenerarInformeTorneos(fechaDesde, fechaHasta);
            ViewData["FechaDesde"] = fechaDesde?.ToString("yyyy-MM-dd");
            ViewData["FechaHasta"] = fechaHasta?.ToString("yyyy-MM-dd");
            return View(modelo);
        }

        /// <summary>
        /// Genera los datos del ViewModel para el informe
        /// </summary>
        private InformeTorneosViewModel GenerarInformeTorneos(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Consulta base de torneos, participaciones y ganadores
            var torneos = _context.Torneos.Include(t => t.TorneoClientes).AsQueryable();
            var participaciones = _context.TorneoClientes.AsQueryable();
            var ganadores = _context.GanadoresTorneos.AsQueryable();

            // Filtro por fecha
            if (fechaDesde.HasValue)
            {
                var desde = DateOnly.FromDateTime(fechaDesde.Value);
                torneos = torneos.Where(t => t.Fecha >= desde);
            }
            if (fechaHasta.HasValue)
            {
                var hasta = DateOnly.FromDateTime(fechaHasta.Value);
                torneos = torneos.Where(t => t.Fecha <= hasta);
            }

            var listaT = torneos.ToList();
            var ids = listaT.Select(t => t.Id).ToList();
            var listaPart = participaciones.Where(p => ids.Contains(p.IdTorneo)).ToList();
            var listaGan = ganadores.Where(g => ids.Contains(g.IdTorneo)).ToList();

            // KPIs
            var totalClientes = _context.Clientes.Count();
            var clientesConCompras = _context.Venta.Select(v => v.IdCliente).Distinct().Count();
            var clientesEnTorneos = listaPart.Select(p => p.IdCliente).Distinct().Count();
            var clientesGanadores = listaGan.Select(g => g.IdCliente).Distinct().Count();

            // Top IDs y nombres
            var idTopPart = listaPart.GroupBy(p => p.IdCliente).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            var idTopGan = listaGan.GroupBy(g => g.IdCliente).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            var topPart = idTopPart != 0 ? $"{_context.Clientes.Find(idTopPart)?.Nombre} {_context.Clientes.Find(idTopPart)?.ApellidoPaterno}" : "N/A";
            var topGan = idTopGan != 0 ? $"{_context.Clientes.Find(idTopGan)?.Nombre} {_context.Clientes.Find(idTopGan)?.ApellidoPaterno}" : "N/A";

            // Día más activo
            var diaActivo = listaT
                .GroupBy(t => t.Fecha.DayOfWeek)
                .Select(g => new {
                    Dia = CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetDayName(g.Key),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault()?.Dia ?? "N/A";

            // Ingresos por entradas
            var ingresos = listaT.Sum(t => t.Entrada * listaPart.Count(p => p.IdTorneo == t.Id));

            // Datos para gráficos
            var participDia = listaT
                .GroupBy(t => t.Fecha.DayOfWeek)
                .Select(g => new TorneoPorDiaDTO
                {
                    Dia = CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetDayName(g.Key),
                    // Sumamos las participaciones de cada torneo t en el grupo
                    Participaciones = g.Sum(t => listaPart.Count(p => p.IdTorneo == t.Id))
                })
                .ToList();


            var ingMensual = listaT
                .GroupBy(t => new { t.Fecha.Year, t.Fecha.Month })
                .Select(g => new TorneoIngresoMensualDTO
                {
                    Mes = CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetMonthName(g.Key.Month) + " " + g.Key.Year,
                    TotalIngreso = g.Sum(t => t.Entrada * listaPart.Count(p => p.IdTorneo == t.Id))
                }).OrderBy(x => x.Mes).ToList();

            var topP = listaPart
                .GroupBy(p => p.IdCliente)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new TopClienteDTO
                {
                    NombreCompleto = $"{_context.Clientes.Find(g.Key)?.Nombre} {_context.Clientes.Find(g.Key)?.ApellidoPaterno}",
                    Total = g.Count()
                }).ToList();

            var topG = listaGan
                .GroupBy(g => g.IdCliente)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new TopClienteDTO
                {
                    NombreCompleto = $"{_context.Clientes.Find(g.Key)?.Nombre} {_context.Clientes.Find(g.Key)?.ApellidoPaterno}",
                    Total = g.Count()
                }).ToList();

            return new InformeTorneosViewModel
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                TotalClientes = totalClientes,
                ClientesConCompras = clientesConCompras,
                ClientesEnTorneos = clientesEnTorneos,
                ClientesGanadores = clientesGanadores,
                ClienteTopParticipaciones = topPart,
                ClienteTopGanador = topGan,
                DiaMasPopular = diaActivo,
                IngresoPorEntradas = ingresos,
                ParticipacionesPorDia = participDia,
                IngresosMensuales = ingMensual,
                TopParticipantes = topP,
                TopGanadores = topG
            };
        }


        public IActionResult ExportarPdf(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var model = GenerarInformeTorneos(fechaDesde, fechaHasta);

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Fuentes
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Título
            document.Add(new Paragraph("Informe Torneos y Clientes")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER));

            // Fechas
            var desdeText = fechaDesde.HasValue ? fechaDesde.Value.ToString("yyyy-MM-dd") : "Inicio";
            var hastaText = fechaHasta.HasValue ? fechaHasta.Value.ToString("yyyy-MM-dd") : "Fin";
            document.Add(new Paragraph($"Desde: {desdeText}    Hasta: {hastaText}")
                .SetFont(normalFont)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10));

            // Tabla Resumen
            var resumen = new Table(2).UseAllAvailableWidth();
            void ResumenRow(string etiqueta, string valor)
            {
                resumen.AddCell(new Cell().Add(new Paragraph(etiqueta).SetFont(boldFont)));
                resumen.AddCell(new Cell().Add(new Paragraph(valor).SetFont(normalFont)));
            }

            ResumenRow("Total Clientes", model.TotalClientes.ToString());
            ResumenRow("Clientes con Compras", model.ClientesConCompras.ToString());
            ResumenRow("Participaron en Torneos", model.ClientesEnTorneos.ToString());
            ResumenRow("Ganadores", model.ClientesGanadores.ToString());
            ResumenRow("+ Participaciones", model.ClienteTopParticipaciones);
            ResumenRow("+ Ganador", model.ClienteTopGanador);
            ResumenRow("Día más Activo", model.DiaMasPopular);
            ResumenRow("Ingreso por Entradas (Bs.)", model.IngresoPorEntradas.ToString("N2"));

            document.Add(resumen);
            document.Add(new Paragraph("\n"));

            // Función para tablas de detalle
            void AddDetailTable<T>(string titulo, string[] headers, IEnumerable<T> items, Func<T, string[]> rowSelector)
            {
                document.Add(new Paragraph(titulo)
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginBottom(5));

                var table = new Table(headers.Length).UseAllAvailableWidth();
                foreach (var h in headers)
                    table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(boldFont)));

                foreach (var item in items)
                {
                    foreach (var cell in rowSelector(item))
                        table.AddCell(new Cell().Add(new Paragraph(cell).SetFont(normalFont)));
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));
            }

            // Participaciones por Día
            AddDetailTable("Participaciones por Día",
                new[] { "Día", "Participaciones" },
                model.ParticipacionesPorDia,
                x => new[] { x.Dia, x.Participaciones.ToString() });

            // Ingresos Mensuales
            AddDetailTable("Ingresos Mensuales",
                new[] { "Mes", "Ingreso (Bs.)" },
                model.IngresosMensuales,
                x => new[] { x.Mes, x.TotalIngreso.ToString("N2") });

            // Top Participantes
            AddDetailTable("Top Participantes",
                new[] { "Cliente", "Participaciones" },
                model.TopParticipantes,
                x => new[] { x.NombreCompleto, x.Total.ToString() });

            // Top Ganadores
            AddDetailTable("Top Ganadores",
                new[] { "Cliente", "Victorias" },
                model.TopGanadores,
                x => new[] { x.NombreCompleto, x.Total.ToString() });

            document.Close();

            var bytes = ms.ToArray();
            return File(bytes, "application/pdf", "InformeTorneos.pdf");
        }

        /// <summary>
        /// Exporta a Excel
        /// </summary>
        public IActionResult ExportarExcel(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var m = GenerarInformeTorneos(fechaDesde, fechaHasta);
            using var wb = new XLWorkbook();

            // Hoja 1: Resumen
            var ws1 = wb.Worksheets.Add("Resumen");
            ws1.Cell(1, 1).Value = "Informe Torneos y Clientes";
            ws1.Range("A1:B1").Merge().Style.Font.SetBold().Font.FontSize = 16;

            ws1.Cell(3, 1).Value = "Total Clientes"; ws1.Cell(3, 2).Value = m.TotalClientes;
            ws1.Cell(4, 1).Value = "Clientes con Compras"; ws1.Cell(4, 2).Value = m.ClientesConCompras;
            ws1.Cell(5, 1).Value = "Participaron en Torneos"; ws1.Cell(5, 2).Value = m.ClientesEnTorneos;
            ws1.Cell(6, 1).Value = "Ganadores"; ws1.Cell(6, 2).Value = m.ClientesGanadores;
            ws1.Cell(7, 1).Value = "+ Participaciones"; ws1.Cell(7, 2).Value = m.ClienteTopParticipaciones;
            ws1.Cell(8, 1).Value = "+ Ganador"; ws1.Cell(8, 2).Value = m.ClienteTopGanador;
            ws1.Cell(9, 1).Value = "Día más Activo"; ws1.Cell(9, 2).Value = m.DiaMasPopular;
            ws1.Cell(10, 1).Value = "Ingreso por Entradas (Bs.)"; ws1.Cell(10, 2).Value = m.IngresoPorEntradas;

            // Hoja 2: Participaciones por Día
            var ws2 = wb.Worksheets.Add("ParticipacionesDía");
            ws2.Cell(1, 1).Value = "Día"; ws2.Cell(1, 2).Value = "Participaciones";
            for (int i = 0; i < m.ParticipacionesPorDia.Count; i++)
            {
                ws2.Cell(i + 2, 1).Value = m.ParticipacionesPorDia[i].Dia;
                ws2.Cell(i + 2, 2).Value = m.ParticipacionesPorDia[i].Participaciones;
            }

            // Hoja 3: Ingresos Mensuales
            var ws3 = wb.Worksheets.Add("IngresosMensuales");
            ws3.Cell(1, 1).Value = "Mes"; ws3.Cell(1, 2).Value = "Ingreso (Bs.)";
            for (int i = 0; i < m.IngresosMensuales.Count; i++)
            {
                ws3.Cell(i + 2, 1).Value = m.IngresosMensuales[i].Mes;
                ws3.Cell(i + 2, 2).Value = m.IngresosMensuales[i].TotalIngreso;
            }

            // Hoja 4: Top Participantes
            var ws4 = wb.Worksheets.Add("TopParticipantes");
            ws4.Cell(1, 1).Value = "Cliente"; ws4.Cell(1, 2).Value = "Participaciones";
            for (int i = 0; i < m.TopParticipantes.Count; i++)
            {
                ws4.Cell(i + 2, 1).Value = m.TopParticipantes[i].NombreCompleto;
                ws4.Cell(i + 2, 2).Value = m.TopParticipantes[i].Total;
            }

            // Hoja 5: Top Ganadores
            var ws5 = wb.Worksheets.Add("TopGanadores");
            ws5.Cell(1, 1).Value = "Cliente"; ws5.Cell(1, 2).Value = "Victorias";
            for (int i = 0; i < m.TopGanadores.Count; i++)
            {
                ws5.Cell(i + 2, 1).Value = m.TopGanadores[i].NombreCompleto;
                ws5.Cell(i + 2, 2).Value = m.TopGanadores[i].Total;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       "InformeTorneos.xlsx");
        }







    }
}
