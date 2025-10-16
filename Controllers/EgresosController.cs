using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class EgresosController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EgresosController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
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
            return _context.Permisos.Any(p => p.IdRol == rol.Id && p.Modulo == modulo && p.Accion == accion && p.Permitido);
        }

        // Egresos
        public async Task<IActionResult> Index()
        {
            if (!TienePermiso("Egreso", "Ver")) return Forbid();

            var egresos = await _context.Egresos
                .Include(e => e.IdTipoEgresoNavigation)
                .Include(e => e.IdUsuarioNavigation)
                .ToListAsync();
            return View(egresos);
        }

        public IActionResult Create()
        {
            if (!TienePermiso("Egreso", "Crear")) return Forbid();
            ViewBag.TipoEgresos = _context.TipoEgresos.ToList();
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.IdUsuario = idUsuario;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int idTipoEgreso, decimal monto, DateOnly? fecha, string? descripcion)
        {
            if (!TienePermiso("Egreso", "Crear"))
                return Forbid();

            if (idTipoEgreso <= 0 || monto <= 0 || !fecha.HasValue)
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios y válidos.");
                ViewBag.TipoEgresos = _context.TipoEgresos.ToList();
                var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                ViewBag.IdUsuario = idUsuario;
                return View();
            }

            try
            {
                var nuevo = new Egreso
                {
                    IdTipoEgreso = idTipoEgreso,
                    IdUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    Monto = monto,
                    Fecha = fecha,
                    Descripcion = descripcion?.Trim()
                };

                _context.Egresos.Add(nuevo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar egreso: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error al guardar.");
                ViewBag.TipoEgresos = _context.TipoEgresos.ToList();
                var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                ViewBag.IdUsuario = idUsuario;
                return View();
            }
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (!TienePermiso("Egreso", "Editar")) return Forbid();
            if (id == null) return NotFound();

            var egreso = await _context.Egresos.FindAsync(id);
            if (egreso == null) return NotFound();

            ViewBag.TipoEgresos = _context.TipoEgresos.ToList();
            return View(egreso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int idTipoEgreso, decimal monto, DateOnly? fecha, string? descripcion)
        {
            if (!TienePermiso("Egreso", "Editar"))
                return Forbid();

            var egresoExistente = await _context.Egresos.FindAsync(id);
            if (egresoExistente == null)
                return NotFound();

            if (idTipoEgreso <= 0 || monto <= 0 || !fecha.HasValue)
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios y válidos.");
                ViewBag.TipoEgresos = _context.TipoEgresos.ToList();
                return View(egresoExistente);
            }

            try
            {
                egresoExistente.IdTipoEgreso = idTipoEgreso;
                egresoExistente.Monto = monto;
                egresoExistente.Fecha = fecha;
                egresoExistente.Descripcion = descripcion?.Trim();
                egresoExistente.IdUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                _context.Update(egresoExistente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"❌ Error al actualizar egreso: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error al actualizar.");
                ViewBag.TipoEgresos = _context.TipoEgresos.ToList();
                return View(egresoExistente);
            }
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (!TienePermiso("Egreso", "Ver")) return Forbid();
            if (id == null) return NotFound();

            var egreso = await _context.Egresos
                .Include(e => e.IdTipoEgresoNavigation)
                .Include(e => e.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (egreso == null) return NotFound();

            return View(egreso);
        }

        [HttpGet]
        public JsonResult BuscarTipoEgreso(string term)
        {
            if (!TienePermiso("Egreso", "Crear")) return Json(null);
            term ??= string.Empty;
            var tipos = _context.TipoEgresos
                .Where(te => te.Nombre.Contains(term))
                .OrderBy(te => te.Nombre)
                .Select(te => new { id = te.Id, nombre = te.Nombre })
                .ToList();
            return Json(tipos);
        }

        // TipoEgresos
        public async Task<IActionResult> IndexTipos()
        {
            if (!TienePermiso("TipoEgreso", "Ver")) return Forbid();
            var tipos = await _context.TipoEgresos.ToListAsync();
            return View("~/Views/TipoEgresos/Index.cshtml", tipos);
        }

        public IActionResult CreateTipo()
        {
            if (!TienePermiso("TipoEgreso", "Crear")) return Forbid();
            return View("~/Views/TipoEgresos/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTipo(TipoEgreso tipoEgreso)
        {
            if (!TienePermiso("TipoEgreso", "Crear")) return Forbid();

            if (ModelState.IsValid)
            {
                _context.TipoEgresos.Add(tipoEgreso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexTipos));
            }
            return View("~/Views/TipoEgresos/Create.cshtml", tipoEgreso);
        }

        public async Task<IActionResult> EditTipo(int? id)
        {
            if (!TienePermiso("TipoEgreso", "Editar")) return Forbid();
            if (id == null) return NotFound();

            var tipoEgreso = await _context.TipoEgresos.FindAsync(id);
            if (tipoEgreso == null) return NotFound();

            return View("~/Views/TipoEgresos/Edit.cshtml", tipoEgreso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTipo(int id, TipoEgreso tipoEgreso)
        {
            if (!TienePermiso("TipoEgreso", "Editar")) return Forbid();

            if (id != tipoEgreso.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoEgreso);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(IndexTipos));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TipoEgresos.Any(te => te.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View("~/Views/TipoEgresos/Edit.cshtml", tipoEgreso);
        }
    }
}
