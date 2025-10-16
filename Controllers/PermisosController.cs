using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class PermisosController : Controller
    {
        private readonly SharksDbContext _context;

        public PermisosController(SharksDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? rol)
        {
            var roles = await _context.Rols
                .Where(r => r.Nombre != "Administrador") // Administrador no editable
                .ToListAsync();

            // Si es Encargado, tampoco puede editar a Encargado
            if (User.IsInRole("Encargado"))
                roles = roles.Where(r => r.Nombre != "Encargado").ToList();

            ViewBag.Roles = roles;
            ViewBag.RolSeleccionado = rol;
            ViewBag.RolActual = User.IsInRole("Administrador") ? "Administrador" : "Encargado";

            if (rol == null)
                return View(new List<Permiso>());

            // Solo trabajaremos con el módulo Cliente por ahora
            var accionesPorModulo = new Dictionary<string, List<string>>
{
    { "Cliente",          new List<string> { "Crear", "Ver", "Editar", "Eliminar" } },
    { "Categoria",        new List<string> { "Crear", "Ver", "Editar", "Eliminar" } },
    { "Codigo",           new List<string> { "Crear", "Ver", "Editar", "Eliminar" } },
    { "Producto",         new List<string> { "Crear", "Ver", "Editar", "Eliminar" } },
    { "Venta",            new List<string> { "Crear", "Ver" } },
    { "VentasAcumulativas", new List<string> { "Crear", "Ver" } },
    // Permisos para el módulo Egreso
{ "Egreso", new List<string> { "Ver", "Crear", "Editar" } },

// Permisos para el módulo TipoEgreso
{ "TipoEgreso", new List<string> { "Ver", "Crear", "Editar" } },

    { "GanadoresTorneo",  new List<string> { "Registrar", "Ver" } },
    { "Informe",          new List<string> { "Ver" } },
    { "InformeTorneos",   new List<string> { "Ver" } },
    { "Torneo",           new List<string> { "Crear", "Ver", "Editar", "Eliminar" } },
    


};


            ViewBag.AccionesPorModulo = accionesPorModulo;

            // Obtener los permisos existentes para ese rol
            var permisosExistentes = await _context.Permisos
                .Where(p => p.IdRol == rol)
                .ToListAsync();

            // Asegurar que existan todos los permisos definidos
            var nuevosPermisos = new List<Permiso>();
            foreach (var modulo in accionesPorModulo.Keys)
            {
                foreach (var accion in accionesPorModulo[modulo])
                {
                    var existente = permisosExistentes
                        .FirstOrDefault(p => p.Modulo == modulo && p.Accion == accion);
                    if (existente == null)
                    {
                        var nuevo = new Permiso
                        {
                            IdRol = rol.Value,
                            Modulo = modulo,
                            Accion = accion,
                            Permitido = false
                        };
                        _context.Permisos.Add(nuevo);
                        nuevosPermisos.Add(nuevo);
                    }
                }
            }

            if (nuevosPermisos.Any())
                await _context.SaveChangesAsync();

            var permisosFinales = await _context.Permisos
                .Where(p => p.IdRol == rol)
                .ToListAsync();

            return View(permisosFinales);
        }

        [HttpPost]
        public async Task<IActionResult> Guardar(int idRol, List<string> permisos)
        {
            // Obtener todos los permisos actuales del rol
            var permisosActuales = await _context.Permisos
                .Where(p => p.IdRol == idRol)
                .ToListAsync();

            foreach (var permiso in permisosActuales)
            {
                string clave = $"{permiso.Modulo}|{permiso.Accion}";
                permiso.Permitido = permisos.Contains(clave);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { rol = idRol });
        }
    }
}
