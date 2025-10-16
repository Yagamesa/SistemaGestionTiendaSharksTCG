using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientesController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Verifica si el usuario tiene permiso
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

        // GET: Clientes
        public async Task<IActionResult> Index(string? busqueda, int page = 1)
        {
            if (!TienePermiso("Cliente", "Ver"))
                return Forbid();

            int pageSize = 10;

            var query = _context.Clientes.AsQueryable();

            // Filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(c =>
                    c.Nombre.Contains(busqueda) ||
                    c.ApellidoPaterno.Contains(busqueda) ||
                    c.ApellidoMaterno.Contains(busqueda) ||
                    c.Ci.Contains(busqueda));
            }

            // Paginación
            int totalRegistros = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRegistros / pageSize);

            var clientes = await query
                .OrderBy(c => c.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.FiltroBusqueda = busqueda;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(clientes);
        }


        // GET: Clientes/Create
        public IActionResult Create()
        {
            if (!TienePermiso("Cliente", "Crear"))
                return Forbid();

            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (!TienePermiso("Cliente", "Crear"))
                return Forbid();

            // Si el campo CI está vacío o solo contiene espacios
            if (string.IsNullOrWhiteSpace(cliente.Ci))
            {
                // Genera un valor único usando un prefijo y la hora actual
                cliente.Ci = "Nulo-" + DateTime.Now.Ticks.ToString();
            }

            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();

                // Redirige al Create de CódigoTCG con el cliente ya seleccionado
                TempData["ClienteNombre"] = $"{cliente.Nombre} {cliente.ApellidoPaterno} {cliente.ApellidoMaterno}";
                return RedirectToAction("Create", "Codigos", new { idCliente = cliente.Id });
            }
            return View(cliente);
        }


        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!TienePermiso("Cliente", "Editar"))
                return Forbid();

            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (!TienePermiso("Cliente", "Editar"))
                return Forbid();

            if (id != cliente.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!TienePermiso("Cliente", "Eliminar"))
                return Forbid();

            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!TienePermiso("Cliente", "Eliminar"))
                return Forbid();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

        // Aquí podrías tener otras acciones como Details o VerCodigos que no necesitan permiso
        // Ejemplo:
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }
        // En el controlador ClientesController

        // 1. Método para redirigir a los códigos del cliente
        public IActionResult VerCodigos(int id)
        {
            return RedirectToAction("Index", "Codigos", new { idCliente = id });
        }

        // 2. Método para mostrar el formulario de SharkCoins
        public async Task<IActionResult> UsarSharkcoins(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }


        // 3. Método para procesar el descuento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DescontarSharkcoins(int id, [FromBody] SharkcoinsRequest data)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            if (data.SharkcoinsDescontar <= 0)
                return BadRequest(new { mensaje = "El monto debe ser mayor a cero." });

            if (cliente.SharkCoins < data.SharkcoinsDescontar)
                return BadRequest(new { mensaje = "El cliente no tiene suficientes SharkCoins." });

            cliente.SharkCoins -= data.SharkcoinsDescontar;
            _context.Update(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Se descontaron {data.SharkcoinsDescontar} SharkCoins correctamente." });
        }

        public class SharkcoinsRequest
        {
            public decimal SharkcoinsDescontar { get; set; }
        }



    }
}
