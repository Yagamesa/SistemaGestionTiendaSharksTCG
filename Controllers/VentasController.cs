using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Text.Json;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string PRODUCTOS_SESSION_KEY = "ProductosVenta";

        public VentasController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
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
            LimpiarVector();
            ViewBag.IdUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return View();
        }

        [HttpGet]
        public JsonResult BuscarClientes(string term)
        {
            if (!TienePermiso("Venta", "Crear") && !TienePermiso("Venta", "Editar")) return Json(null);

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
            if (!TienePermiso("Venta", "Crear") && !TienePermiso("Venta", "Editar")) return Json(null);

            term ??= string.Empty;
            var productos = _context.Productos
                .Where(p => p.Nombre.Contains(term))
                .Select(p => new { id = p.Id, nombre = p.Nombre, precio = p.PrecioVenta })
                .ToList();
            return Json(productos);
        }

        [HttpPost]
        public IActionResult AgregarProducto([FromBody] ProductoVentaDTO dto)
        {
            if (!TienePermiso("Venta", "Crear") && !TienePermiso("Venta", "Editar")) return Forbid();

            if (dto == null) return BadRequest("Datos inválidos");
            var lista = ObtenerVector();
            lista.RemoveAll(x => x.IdProducto == dto.IdProducto);
            lista.Add(dto);
            GuardarVector(lista);
            return Ok();
        }

        [HttpPost]
        public IActionResult EliminarProducto([FromBody] int idProducto)
        {
            if (!TienePermiso("Venta", "Crear") && !TienePermiso("Venta", "Editar")) return Forbid();

            var lista = ObtenerVector();
            lista.RemoveAll(x => x.IdProducto == idProducto);
            GuardarVector(lista);
            return Ok();
        }

        [HttpGet]
        public IActionResult ObtenerProductos()
        {
            if (!TienePermiso("Venta", "Crear") && !TienePermiso("Venta", "Editar")) return Forbid();

            var lista = ObtenerVector();
            var datos = lista.Select(p => new
            {
                p.IdProducto,
                ProductoNombre = _context.Productos.FirstOrDefault(prod => prod.Id == p.IdProducto)?.Nombre,
                p.Cantidad,
                p.PrecioUnitario,
                p.TipoPago,
                Total = p.Cantidad * p.PrecioUnitario
            }).ToList();
            return Json(datos);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarVenta(Venta venta, decimal descuento, decimal pago)
        {
            if (!TienePermiso("Venta", "Crear")) return Forbid();

            ModelState.Remove(nameof(venta.IdClienteNavigation));
            ModelState.Remove(nameof(venta.IdUsuarioNavigation));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var productos = ObtenerVector();
            if (!productos.Any())
                return BadRequest("Debe registrar al menos un producto.");

            // Calcular total bruto
            decimal totalBruto = productos.Sum(p => p.Cantidad * p.PrecioUnitario);
            decimal totalFinal = totalBruto - descuento;
            decimal deuda = totalFinal - pago;
            if (deuda < 0) deuda = 0;

            // Verificar cliente
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == venta.IdCliente);
            if (cliente == null) return BadRequest("Cliente no válido.");

            // Aumentar deuda al cliente
            cliente.Deuda += deuda;

            // Asignar valores a la venta
            venta.FechaVenta = DateOnly.FromDateTime(DateTime.Now);
            venta.Total = totalFinal;
            venta.Descuento = descuento;
            venta.Pago = pago;

            _context.Venta.Add(venta);
            await _context.SaveChangesAsync();

            // Registrar productos y salida de stock
            foreach (var p in productos)
            {
                var stockActual = await _context.Stocks
                    .Where(s => s.IdProducto == p.IdProducto)
                    .SumAsync(s => s.TipoMovimiento == "Entrada" ? s.Cantidad : -s.Cantidad);

                if (stockActual < p.Cantidad)
                    return BadRequest($"Stock insuficiente para el producto ID {p.IdProducto}.");

                _context.ProductoVenta.Add(new ProductoVenta
                {
                    IdVenta = venta.Id,
                    IdProducto = p.IdProducto,
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario,
                    TipoPago = p.TipoPago
                });

                _context.Stocks.Add(new Stock
                {
                    IdProducto = p.IdProducto,
                    Cantidad = p.Cantidad,
                    TipoMovimiento = "Salida",
                    FechaMovimiento = DateTime.Now,
                    Descripcion = $"Salida por venta ID {venta.Id}"
                });
            }

            await _context.SaveChangesAsync();
            LimpiarVector();

            // Pasar info a TempData (por si deseas mostrar en el Details)
            TempData["Descuento"] = descuento.ToString("0.00");
            TempData["Pago"] = pago.ToString("0.00");
            TempData["TotalBruto"] = totalBruto.ToString("0.00");
            TempData["TotalFinal"] = totalFinal.ToString("0.00");
            TempData["Deuda"] = deuda.ToString("0.00");


            return RedirectToAction("Details", new { id = venta.Id });
        }


        [HttpPost]
        public IActionResult CancelarVenta()
        {
            if (!TienePermiso("Venta", "Crear")) return Forbid();
            LimpiarVector();
            return Ok();
        }

        public async Task<IActionResult> Index(int page = 1, string? cliente = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            if (!TienePermiso("Venta", "Ver")) return Forbid();

            int pageSize = 10;

            var query = _context.Venta
                .Include(v => v.IdClienteNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cliente))
                query = query.Where(v =>
                    (v.IdClienteNavigation.Nombre + " " + v.IdClienteNavigation.ApellidoPaterno + " " + v.IdClienteNavigation.ApellidoMaterno)
                    .Contains(cliente));

            if (fechaInicio.HasValue)
                query = query.Where(v => v.FechaVenta >= DateOnly.FromDateTime(fechaInicio.Value));

            if (fechaFin.HasValue)
                query = query.Where(v => v.FechaVenta <= DateOnly.FromDateTime(fechaFin.Value));

            var totalVentas = await query.CountAsync();

            var ventas = await query
                .OrderByDescending(v => v.FechaVenta)
                .ThenByDescending(v => v.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalVentas / (double)pageSize);
            ViewBag.FiltroCliente = cliente;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View(ventas);
        }


        public async Task<IActionResult> Details(int id)
        {
            if (!TienePermiso("Venta", "Ver")) return Forbid();

            var venta = await _context.Venta
                .Include(v => v.IdClienteNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null) return NotFound();

            var detalles = await _context.ProductoVenta
                .Where(d => d.IdVenta == id)
                .Include(d => d.IdProductoNavigation)
                .ToListAsync();

            ViewBag.Detalles = detalles;

            // Ahora tomamos directamente los datos guardados en la venta
            ViewBag.Descuento = venta.Descuento;
            ViewBag.Pago = venta.Pago;
            ViewBag.TotalFinal = venta.Total;

            // Calculamos total bruto desde los detalles
            decimal totalBruto = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            ViewBag.TotalBruto = totalBruto;

            // Calculamos deuda desde los campos
            decimal deuda = venta.Total - venta.Pago;
            if (deuda < 0) deuda = 0;
            ViewBag.Deuda = deuda;


            return View(venta);
        }


        private List<ProductoVentaDTO> ObtenerVector()
        {
            var json = HttpContext.Session.GetString(PRODUCTOS_SESSION_KEY);
            if (string.IsNullOrEmpty(json)) return new List<ProductoVentaDTO>();
            return JsonSerializer.Deserialize<List<ProductoVentaDTO>>(json)!;
        }

        private void GuardarVector(List<ProductoVentaDTO> lista)
        {
            var json = JsonSerializer.Serialize(lista);
            HttpContext.Session.SetString(PRODUCTOS_SESSION_KEY, json);
        }

        private void LimpiarVector() => HttpContext.Session.Remove(PRODUCTOS_SESSION_KEY);

        public class ProductoVentaDTO
        {
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public string TipoPago { get; set; } = null!;
        }


        //Metodos eliminar y editar
        //Editar
        // Dentro de VentasController.cs

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!TienePermiso("Venta", "Editar")) return Forbid();

            if (id == null) return NotFound();

            var venta = await _context.Venta
                .Include(v => v.IdClienteNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.ProductoVenta)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null) return NotFound();

            // Cargar productos originales a la sesión
            var vector = venta.ProductoVenta.Select(p => new ProductoVentaDTO
            {
                IdProducto = p.IdProducto,
                Cantidad = p.Cantidad,
                PrecioUnitario = p.PrecioUnitario,
                TipoPago = p.TipoPago
            }).ToList();
            GuardarVector(vector);

            return View(venta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int idCliente, decimal descuento, decimal pago)
        {
            if (!TienePermiso("Venta", "Editar")) return Forbid();

            var venta = await _context.Venta
                .Include(v => v.IdClienteNavigation)
                .Include(v => v.ProductoVenta)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null) return NotFound();

            var productos = ObtenerVector();
            if (!productos.Any()) return BadRequest("Debe agregar al menos un producto.");

            // Revertir deuda anterior
            var deudaAnterior = venta.Total - venta.Pago;
            if (deudaAnterior < 0) deudaAnterior = 0;
            venta.IdClienteNavigation.Deuda -= deudaAnterior;
            if (venta.IdClienteNavigation.Deuda < 0) venta.IdClienteNavigation.Deuda = 0;

            // Calcular nuevos valores
            decimal totalBruto = productos.Sum(p => p.Cantidad * p.PrecioUnitario);
            decimal totalFinal = totalBruto - descuento;
            decimal nuevaDeuda = totalFinal - pago;
            if (nuevaDeuda < 0) nuevaDeuda = 0;

            // Aplicar nueva deuda
            venta.IdClienteNavigation.Deuda += nuevaDeuda;

            // Actualizar venta
            venta.IdCliente = idCliente;
            venta.FechaVenta = DateOnly.FromDateTime(DateTime.Now);
            venta.Total = totalFinal;
            venta.Descuento = descuento;
            venta.Pago = pago;

            // Actualizar detalles
            var productosAnteriores = venta.ProductoVenta.ToList();
            _context.ProductoVenta.RemoveRange(productosAnteriores);

            foreach (var p in productos)
            {
                _context.ProductoVenta.Add(new ProductoVenta
                {
                    IdVenta = venta.Id,
                    IdProducto = p.IdProducto,
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario,
                    TipoPago = p.TipoPago
                });
            }

            await _context.SaveChangesAsync();
            LimpiarVector();

            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!TienePermiso("Venta", "Eliminar")) return Forbid();

            var venta = await _context.Venta
                .Include(v => v.ProductoVenta)
                .Include(v => v.IdClienteNavigation)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            // Calcular deuda registrada en la venta
            decimal deudaVenta = venta.Total - venta.Pago;
            if (deudaVenta < 0) deudaVenta = 0;

            // Ajustar deuda del cliente (restar lo que se le había sumado al registrar esta venta)
            venta.IdClienteNavigation.Deuda -= deudaVenta;
            if (venta.IdClienteNavigation.Deuda < 0)
                venta.IdClienteNavigation.Deuda = 0;

            // Eliminar productos asociados
            if (venta.ProductoVenta.Any())
            {
                _context.ProductoVenta.RemoveRange(venta.ProductoVenta);
            }

            // Eliminar la venta
            _context.Venta.Remove(venta);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }



}
