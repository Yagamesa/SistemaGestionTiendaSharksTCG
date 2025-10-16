using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;

namespace ProyectoMDGSharksWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SharksDbContext _context;

        public HomeController(ILogger<HomeController> logger, SharksDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ✅ Verifica si es la primera vez (no hay roles ni usuarios)
            bool sinUsuarios = !await _context.Users.AnyAsync();
            bool sinRoles = !await _context.Rols.AnyAsync();

            if (sinUsuarios || sinRoles)
            {
                return RedirectToAction("InicializarSistema", "Users");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult MostrarAccesoDenegado()
        {
            TempData.Keep("AccesoDenegado");
            return View();
        }

    }
}
