using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly ILogger<ProductosController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const int PageSize = 10;

        public ProductosController(SharksDbContext context, ILogger<ProductosController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
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

        // GET: Productos
        public async Task<IActionResult> Index(string? search, int? categoriaId, int page = 1)
        {
            if (!TienePermiso("Producto", "Ver")) return Forbid();

            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.Stocks)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Nombre.Contains(search));
            }

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query = query.Where(p => p.IdCategoria == categoriaId.Value);
            }

            var totalItems = await query.CountAsync();
            var productos = await query
                .OrderBy(p => p.Nombre)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.Search = search;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.Categorias = new SelectList(_context.Categoria.OrderBy(c => c.Nombre), "Id", "Nombre");

            return View(productos);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (!TienePermiso("Producto", "Ver")) return Forbid();

            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.Stocks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();

            ViewBag.StockActual = producto.Stocks.Sum(s => s.TipoMovimiento == "Entrada" ? s.Cantidad : -s.Cantidad);
            return View(producto);
        }

        public IActionResult Create()
        {
            if (!TienePermiso("Producto", "Crear")) return Forbid();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto, int stockInicial)
        {
            if (!TienePermiso("Producto", "Crear")) return Forbid();

            ModelState.Remove("IdCategoriaNavigation");

            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();

                var movimiento = new Stock
                {
                    IdProducto = producto.Id,
                    Cantidad = stockInicial,
                    TipoMovimiento = "Entrada",
                    FechaMovimiento = DateTime.Now,
                    Descripcion = "Stock inicial"
                };
                _context.Stocks.Add(movimiento);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!TienePermiso("Producto", "Editar")) return Forbid();

            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Stocks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();

            var stockActual = producto.Stocks.Sum(s => s.TipoMovimiento == "Entrada" ? s.Cantidad : -s.Cantidad);
            ViewBag.StockAjustado = stockActual;
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto, int stockAjustado)
        {
            if (!TienePermiso("Producto", "Editar")) return Forbid();

            if (id != producto.Id) return NotFound();
            ModelState.Remove("IdCategoriaNavigation");

            if (!ModelState.IsValid)
            {
                var stockActual = await _context.Stocks
                    .Where(s => s.IdProducto == producto.Id)
                    .SumAsync(s => s.TipoMovimiento == "Entrada" ? s.Cantidad : -s.Cantidad);
                ViewBag.StockAjustado = stockActual;
                return View(producto);
            }

            try
            {
                _context.Update(producto);
                await _context.SaveChangesAsync();

                var stockActual = await _context.Stocks
                    .Where(s => s.IdProducto == producto.Id)
                    .SumAsync(s => s.TipoMovimiento == "Entrada" ? s.Cantidad : -s.Cantidad);
                var diferencia = stockAjustado - stockActual;

                if (diferencia != 0)
                {
                    var movimiento = new Stock
                    {
                        IdProducto = producto.Id,
                        Cantidad = Math.Abs(diferencia),
                        TipoMovimiento = diferencia > 0 ? "Entrada" : "Salida",
                        FechaMovimiento = DateTime.Now,
                        Descripcion = "Ajuste automático en edición"
                    };
                    _context.Stocks.Add(movimiento);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(producto.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!TienePermiso("Producto", "Eliminar")) return Forbid();

            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!TienePermiso("Producto", "Eliminar")) return Forbid();

            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public JsonResult BuscarCategorias(string term)
        {
            term ??= string.Empty;
            var categorias = _context.Categoria
                .Where(c => c.Nombre.Contains(term))
                .OrderBy(c => c.Nombre)
                .Take(10)
                .Select(c => new
                {
                    id = c.Id,
                    nombre = c.Nombre
                })
                .ToList();
            return Json(categorias);
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
