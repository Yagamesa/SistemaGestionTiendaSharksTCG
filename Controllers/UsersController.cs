using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Security.Claims;

namespace ProyectoMDGSharksWeb.Controllers
{
    public class UsersController : Controller
    {
        private readonly SharksDbContext _context;

        public UsersController(SharksDbContext context)
        {
            _context = context;
        }

        private bool UsuarioTieneAcceso()
        {
            var rolNombre = User.FindFirstValue(ClaimTypes.Role);
            return rolNombre == "Administrador" || rolNombre == "Encargado";
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            var usuarios = await _context.Users
                .Include(u => u.IdRolNavigation)
                .ToListAsync();

            return View(usuarios);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            var usuario = await _context.Users
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            bool instalacionInicial = TempData["InstalacionInicial"]?.ToString() == "true";
            int idRolPorDefecto = 0;

            if (!instalacionInicial && !User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Auth");

            var roles = await _context.Rols.ToListAsync();

            if (instalacionInicial)
            {
                roles = roles.Where(r => r.Nombre == "Administrador").ToList();
                idRolPorDefecto = roles.FirstOrDefault()?.Id ?? 0;
            }

            ViewBag.Roles = roles;
            ViewBag.RolIdPorDefecto = idRolPorDefecto;
            TempData.Keep("InstalacionInicial");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create(
            string nombre,
            string apellidoPaterno,
            string apellidoMaterno,
            int idRol,
            string email,
            string password,
            string confirmarPassword)
        {
            bool instalacionInicial = TempData["InstalacionInicial"]?.ToString() == "true";

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Debe completar los campos obligatorios.");
                ViewBag.Roles = await _context.Rols.ToListAsync();
                return View();
            }

            if (password != confirmarPassword)
            {
                ModelState.AddModelError("confirmarPassword", "Las contraseñas no coinciden.");
                ViewBag.Roles = await _context.Rols.ToListAsync();
                return View();
            }

            try
            {
                var rolNombreSolicitado = (await _context.Rols.FindAsync(idRol))?.Nombre ?? "";

                if (!instalacionInicial && rolNombreSolicitado == "Administrador")
                {
                    var rolActual = User.FindFirstValue(ClaimTypes.Role);
                    if (rolActual != "Administrador")
                    {
                        ModelState.AddModelError("", "No tiene permiso para asignar el rol de Administrador.");
                        ViewBag.Roles = await _context.Rols.ToListAsync();
                        return View();
                    }
                }

                var user = new User
                {
                    Nombre = nombre,
                    ApellidoPaterno = apellidoPaterno,
                    ApellidoMaterno = apellidoMaterno,
                    IdRol = idRol,
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password)
                };


                _context.Users.Add(user);

                if (instalacionInicial)
                {
                    var rolesFaltantes = new List<string> { "Encargado", "Vendedor" };
                    foreach (var nombreRol in rolesFaltantes)
                    {
                        if (!await _context.Rols.AnyAsync(r => r.Nombre == nombreRol))
                        {
                            _context.Rols.Add(new Rol
                            {
                                Nombre = nombreRol,
                                Permisos = "Personalizado"
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                var rolNombre = (await _context.Rols.FindAsync(user.IdRol))?.Nombre ?? "SinRol";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Nombre),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, rolNombre)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar: {ex.Message}");
                ModelState.AddModelError("", "Error al guardar el usuario.");
                ViewBag.Roles = await _context.Rols.ToListAsync();
                return View();
            }
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            ViewBag.Roles = await _context.Rols.ToListAsync();
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(
            int id,
            string nombre,
            string apellidoPaterno,
            string apellidoMaterno,
            int idRol,
            string email)
        {
            if (!UsuarioTieneAcceso())
                return Forbid();

            var usuario = await _context.Users.FindAsync(id);
            if (usuario == null)
                return NotFound();

            bool existeOtroConMismoUsuario = await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != id);

            if (existeOtroConMismoUsuario)
            {
                ModelState.AddModelError("email", "Ya existe otro usuario con ese nombre de usuario.");
                ViewBag.Roles = await _context.Rols.ToListAsync();
                return View(usuario);
            }

            // Validación para impedir que usuarios no administradores asignen el rol de Administrador
            var rolNombreSolicitado = (await _context.Rols.FindAsync(idRol))?.Nombre ?? "";
            if (rolNombreSolicitado == "Administrador")
            {
                var rolActual = User.FindFirstValue(ClaimTypes.Role);
                if (rolActual != "Administrador")
                {
                    ModelState.AddModelError("", "No tiene permiso para asignar el rol de Administrador.");
                    ViewBag.Roles = await _context.Rols.ToListAsync();
                    return View(usuario);
                }
            }

            usuario.Nombre = nombre;
            usuario.ApellidoPaterno = apellidoPaterno;
            usuario.ApellidoMaterno = apellidoMaterno;
            usuario.IdRol = idRol;
            usuario.Email = email;

            try
            {
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error actualizando usuario: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Error al actualizar el usuario.");
                ViewBag.Roles = await _context.Rols.ToListAsync();
                return View(usuario);
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public JsonResult VerificarUsuario(string baseUsuario)
        {
            if (string.IsNullOrWhiteSpace(baseUsuario))
                return Json(baseUsuario);

            var nombresExistentes = _context.Users
                .Where(u => u.Email.StartsWith(baseUsuario))
                .Select(u => u.Email)
                .ToList();

            if (!nombresExistentes.Contains(baseUsuario))
                return Json(baseUsuario);

            int sufijo = 1;
            string usuarioFinal;
            do
            {
                usuarioFinal = baseUsuario + sufijo;
                sufijo++;
            } while (nombresExistentes.Contains(usuarioFinal));

            return Json(usuarioFinal);
        }

        [AllowAnonymous]
        public async Task<IActionResult> InicializarSistema()
        {
            bool hayUsuarios = await _context.Users.AnyAsync();
            bool hayRoles = await _context.Rols.AnyAsync();
            int idRolAdmin = 0;

            if (!hayUsuarios)
            {
                if (!hayRoles)
                {
                    var rolAdmin = new Rol { Nombre = "Administrador", Permisos = "Total" };
                    _context.Rols.Add(rolAdmin);
                    await _context.SaveChangesAsync();
                    idRolAdmin = rolAdmin.Id;
                }
                else
                {
                    var admin = await _context.Rols.FirstOrDefaultAsync(r => r.Nombre == "Administrador");
                    if (admin != null)
                        idRolAdmin = admin.Id;
                }

                TempData["InstalacionInicial"] = "true";
                TempData["RolIdPorDefecto"] = idRolAdmin;
                return RedirectToAction("Create");
            }

            return RedirectToAction("Login", "Auth");
        }
    }
}
