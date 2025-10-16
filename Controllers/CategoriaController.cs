using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class CategoriaController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoriaController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
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

        // GET: Categoria
        public async Task<IActionResult> Index()
        {
            if (!TienePermiso("Categoria", "Ver"))
                return Forbid();

            var categorias = await _context.Categoria.ToListAsync();
            return View(categorias);
        }

        // GET: Categoria/Create
        public IActionResult Create()
        {
            if (!TienePermiso("Categoria", "Crear"))
                return Forbid();

            return View();
        }

        // POST: Categoria/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            if (!TienePermiso("Categoria", "Crear"))
                return Forbid();

            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categoria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!TienePermiso("Categoria", "Editar"))
                return Forbid();

            if (id == null) return NotFound();

            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: Categoria/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            if (!TienePermiso("Categoria", "Editar"))
                return Forbid();

            if (id != categoria.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(categoria);
        }

        // GET: Categoria/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!TienePermiso("Categoria", "Eliminar"))
                return Forbid();

            if (id == null) return NotFound();

            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null) return NotFound();

            var tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
            if (tieneProductos)
            {
                TempData["Error"] = "No se puede eliminar la categoría porque tiene productos vinculados.";
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        // POST: Categoria/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!TienePermiso("Categoria", "Eliminar"))
                return Forbid();

            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null) return NotFound();

            var tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
            if (tieneProductos)
            {
                TempData["Error"] = "No se puede eliminar la categoría porque tiene productos vinculados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categoria.Remove(categoria);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categoria.Any(e => e.Id == id);
        }
    }
}
