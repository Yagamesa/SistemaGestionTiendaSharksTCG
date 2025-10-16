using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Security.Claims;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly SharksDbContext _context;

        public RolesController(SharksDbContext context)
        {
            _context = context;
        }

        // Middleware para restringir a Administrador o Encargado
        private bool UsuarioTieneAcceso()
        {
            var rolNombre = User.FindFirstValue(ClaimTypes.Role);
            return rolNombre == "Administrador" || rolNombre == "Encargado";
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            var roles = await _context.Rols.ToListAsync();
            return View(roles);
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            var rol = await _context.Rols.FirstOrDefaultAsync(r => r.Id == id);
            if (rol == null)
                return NotFound();

            return View(rol);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rol rol)
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            if (!ModelState.IsValid)
                return View(rol);

            _context.Add(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            var rol = await _context.Rols.FindAsync(id);
            if (rol == null)
                return NotFound();

            return View(rol);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rol rol)
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            if (id != rol.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(rol);

            _context.Update(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
