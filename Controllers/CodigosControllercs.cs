using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class CodigosController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CodigosController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            if (!TienePermiso("Codigo", "Ver"))
                return Forbid();

            int pageSize = 10;
            var query = _context.CodigoTcgs
                .Include(c => c.IdClienteNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.IdClienteNavigation.Nombre.Contains(search) ||
                    c.IdClienteNavigation.ApellidoPaterno.Contains(search) ||
                    c.IdClienteNavigation.ApellidoMaterno.Contains(search));
            }

            var total = await query.CountAsync();
            var codigos = await query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search; // <-- clave para que se mantenga en el input

            return View(codigos);
        }


        // GET: Codigos/Create
        // GET: Codigos/Create
        public IActionResult Create(int? idCliente)
        {
            if (!TienePermiso("Codigo", "Crear"))
                return Forbid();

            ViewBag.IdCliente = idCliente;

            if (idCliente.HasValue)
            {
                var cliente = _context.Clientes.Find(idCliente.Value);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.ApellidoPaterno} {cliente.ApellidoMaterno}";
                }
            }

            return View();
        }

        // POST: Codigos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int idCliente, string juego, string codigo)
        {
            if (!TienePermiso("Codigo", "Crear"))
                return Forbid();

            if (idCliente <= 0 || string.IsNullOrWhiteSpace(juego) || string.IsNullOrWhiteSpace(codigo))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                return View();
            }

            try
            {
                var nuevo = new CodigoTcg
                {
                    IdCliente = idCliente,
                    Juego = juego.Trim(),
                    Codigo = codigo.Trim()
                };

                _context.CodigoTcgs.Add(nuevo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar código: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error al guardar.");
                return View();
            }
        }

        // GET: Codigos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!TienePermiso("Codigo", "Editar"))
                return Forbid();

            var codigo = await _context.CodigoTcgs.FindAsync(id);
            if (codigo == null)
                return NotFound();

            var cliente = await _context.Clientes.FindAsync(codigo.IdCliente);
            ViewBag.ClienteNombre = cliente != null ? $"{cliente.Nombre} {cliente.ApellidoPaterno} {cliente.ApellidoMaterno}" : "";

            return View(codigo);
        }

        // POST: Codigos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int idCliente, string juego, string codigo)
        {
            if (!TienePermiso("Codigo", "Editar"))
                return Forbid();

            var codigoExistente = await _context.CodigoTcgs.FindAsync(id);
            if (codigoExistente == null)
                return NotFound();

            if (idCliente <= 0 || string.IsNullOrWhiteSpace(juego) || string.IsNullOrWhiteSpace(codigo))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                return View(codigoExistente);
            }

            try
            {
                codigoExistente.IdCliente = idCliente;
                codigoExistente.Juego = juego.Trim();
                codigoExistente.Codigo = codigo.Trim();

                _context.Update(codigoExistente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"❌ Error al actualizar código: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error al actualizar.");
                return View(codigoExistente);
            }
        }

        // GET: Codigos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!TienePermiso("Codigo", "Ver"))
                return Forbid();

            var codigo = await _context.CodigoTcgs
                .Include(c => c.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (codigo == null)
                return NotFound();

            return View(codigo);
        }

        // Buscador dinámico estilo Torneos
        [HttpGet]
        public JsonResult BuscarClientes(string term)
        {
            term ??= string.Empty;
            var clientes = _context.Clientes
                .Where(c => c.Nombre.Contains(term) || c.ApellidoPaterno.Contains(term) || c.ApellidoMaterno.Contains(term))
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    id = c.Id,
                    nombreCompleto = c.Nombre + " " + c.ApellidoPaterno + " " + c.ApellidoMaterno
                })
                .ToList();

            return Json(clientes);
        }
    }
}
