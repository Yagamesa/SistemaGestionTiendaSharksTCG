using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class VentasAcumulativasController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string VENTAS_ACUMULADAS_KEY = "VentasAcumuladas";

        public VentasAcumulativasController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public IActionResult Create()
        {
            if (!TienePermiso("Venta", "Crear")) return Forbid();
            ViewBag.IdUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return View();
        }

        [HttpGet]
        public JsonResult BuscarClientes(string term)
        {
            if (!TienePermiso("Venta", "Crear")) return Json(null);
            term ??= string.Empty;
            var clientes = _context.Clientes
                .Where(c => c.Nombre.Contains(term) || c.ApellidoPaterno.Contains(term) || c.ApellidoMaterno.Contains(term))
                .OrderBy(c => c.Nombre)
                .Select(c => new { id = c.Id, nombreCompleto = c.Nombre + " " + c.ApellidoPaterno + " " + c.ApellidoMaterno })
                .ToList();
            return Json(clientes);
        }

        [HttpGet]
        public JsonResult BuscarProductos(string term)
        {
            if (!TienePermiso("Venta", "Crear")) return Json(null);
            term ??= string.Empty;
            var productos = _context.Productos
                .Where(p => p.Nombre.Contains(term))
                .Select(p => new { id = p.Id, nombre = p.Nombre, precio = p.PrecioVenta })
                .ToList();
            return Json(productos);
        }

        [HttpPost]
        public IActionResult AgregarVentaIndividual([FromBody] VentaAcumulativaDTO dto)
        {
            if (!TienePermiso("Venta", "Crear")) return Forbid();
            if (dto == null) return BadRequest("Datos inválidos");

            var lista = ObtenerVector();
            lista.RemoveAll(x => x.IdTemp == dto.IdTemp); // Para actualizaciones
            lista.Add(dto);
            GuardarVector(lista);
            return Ok();
        }

        [HttpPost]
        public IActionResult EliminarVenta([FromBody] string idTemp)
        {
            if (!TienePermiso("Venta", "Crear")) return Forbid();
            var lista = ObtenerVector();
            lista.RemoveAll(x => x.IdTemp == idTemp);
            GuardarVector(lista);
            return Ok();
        }

        [HttpGet]
        public IActionResult ObtenerVentas()
        {
            if (!TienePermiso("Venta", "Crear")) return Json(null);
            var lista = ObtenerVector();
            var datos = lista.Select(v => new
            {
                v.IdTemp,
                v.IdCliente,
                v.IdProducto,
                ClienteNombre = _context.Clientes.FirstOrDefault(c => c.Id == v.IdCliente)?.Nombre + " " +
                                _context.Clientes.FirstOrDefault(c => c.Id == v.IdCliente)?.ApellidoPaterno,
                ProductoNombre = _context.Productos.FirstOrDefault(p => p.Id == v.IdProducto)?.Nombre,
                v.Cantidad,
                v.PrecioUnitario,
                v.Descuento,
                v.Pago,
                v.TipoPago,
                Total = (v.Cantidad * v.PrecioUnitario) - v.Descuento
            }).ToList();
            return Json(datos);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarTodas()
        {
            if (!TienePermiso("Venta", "Crear")) return Forbid();

            var lista = ObtenerVector();
            if (!lista.Any()) return BadRequest("Debe agregar al menos una venta.");

            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            foreach (var item in lista)
            {
                decimal total = (item.Cantidad * item.PrecioUnitario) - item.Descuento;
                decimal descuento = item.Descuento;
                decimal pago = item.Pago;
                decimal deuda = total - pago;
                if (deuda < 0) deuda = 0;

                var venta = new Venta
                {
                    IdCliente = item.IdCliente,
                    IdUsuario = idUsuario,
                    FechaVenta = fecha,
                    Total = total,
                    Descuento = descuento,
                    Pago = pago
                };

                // Verificar cliente
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == item.IdCliente);
                if (cliente == null)
                    return BadRequest($"Cliente con ID {item.IdCliente} no válido.");

                // Aumentar deuda al cliente
                cliente.Deuda += deuda;
                _context.Clientes.Update(cliente);

                _context.Venta.Add(venta);
                await _context.SaveChangesAsync();

                var stockActual = await _context.Stocks
                    .Where(s => s.IdProducto == item.IdProducto)
                    .SumAsync(s => s.TipoMovimiento == "Entrada" ? s.Cantidad : -s.Cantidad);

                if (stockActual < item.Cantidad)
                    return BadRequest($"Stock insuficiente para el producto ID {item.IdProducto}.");

                _context.ProductoVenta.Add(new ProductoVenta
                {
                    IdVenta = venta.Id,
                    IdProducto = item.IdProducto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    TipoPago = item.TipoPago
                });

                _context.Stocks.Add(new Stock
                {
                    IdProducto = item.IdProducto,
                    Cantidad = item.Cantidad,
                    TipoMovimiento = "Salida",
                    FechaMovimiento = DateTime.Now,
                    Descripcion = $"Salida por venta acumulativa ID {venta.Id}"
                });

                await _context.SaveChangesAsync();
            }

            LimpiarVector();
            return RedirectToAction("Index", "Ventas");
        }

        [HttpPost]
        public IActionResult CancelarAcumuladas()
        {
            if (!TienePermiso("Venta", "Crear")) return Forbid();
            LimpiarVector();
            return Ok();
        }

        private List<VentaAcumulativaDTO> ObtenerVector()
        {
            var json = HttpContext.Session.GetString(VENTAS_ACUMULADAS_KEY);
            if (string.IsNullOrEmpty(json)) return new List<VentaAcumulativaDTO>();
            return JsonSerializer.Deserialize<List<VentaAcumulativaDTO>>(json)!;
        }

        private void GuardarVector(List<VentaAcumulativaDTO> lista)
        {
            var json = JsonSerializer.Serialize(lista);
            HttpContext.Session.SetString(VENTAS_ACUMULADAS_KEY, json);
        }

        private void LimpiarVector() =>
            HttpContext.Session.Remove(VENTAS_ACUMULADAS_KEY);

        public class VentaAcumulativaDTO
        {
            public string IdTemp { get; set; } = Guid.NewGuid().ToString();
            public int IdCliente { get; set; }
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public string TipoPago { get; set; } = null!;

            // Nuevos campos para cada venta individual
            public decimal Descuento { get; set; } = 0m;
            public decimal Pago { get; set; } = 0m;
        }
    }
}
