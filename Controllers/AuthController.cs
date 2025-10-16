using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoMDGSharksWeb.Models;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly SharksDbContext _context;

        public AuthController(SharksDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // Verificar si no hay roles o usuarios en la base de datos
            if (!_context.Rols.Any() || !_context.Users.Any())
            {
                // Crear roles por defecto si no existen
                if (!_context.Rols.Any())
                {
                    _context.Rols.AddRange(
                        new Rol { Nombre = "Administrador", Permisos = "Full" },
                        new Rol { Nombre = "Encargado", Permisos = "Limitado" },
                        new Rol { Nombre = "Vendedor", Permisos = "Ventas" }
                    );
                    await _context.SaveChangesAsync();
                }

                // Redirigir a la vista para registrar el primer administrador
                return RedirectToAction("CreatePrimerAdmin", "Users");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View();
            }

            // Cambiado: se guarda el nombre del rol, no el ID
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.IdRolNavigation.Nombre) // ← aquí
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

        [HttpGet]
        public IActionResult ConfirmLogout()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}
