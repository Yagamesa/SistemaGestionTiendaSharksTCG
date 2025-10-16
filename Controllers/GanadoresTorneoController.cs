using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;
using System.Text.Json;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class GanadoresTorneoController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SESSION_KEY_PREFIX = "GanadoresTorneo_";

        public GanadoresTorneoController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<IActionResult> Registrar(int idTorneo)
        {
            if (!TienePermiso("GanadoresTorneo", "Registrar"))
                return Forbid();

            var participantes = await _context.TorneoClientes
                .Include(tc => tc.IdClienteNavigation)
                .Where(tc => tc.IdTorneo == idTorneo)
                .ToListAsync();

            if (!participantes.Any()) return NotFound("No hay participantes");

            var torneo = await _context.Torneos.FindAsync(idTorneo);
            if (torneo == null) return NotFound("Torneo no encontrado");

            var ganadores = participantes.Select(p => new GanadorDTO
            {
                IdCliente = p.IdCliente,
                NombreCompleto = p.IdClienteNavigation.Nombre + " " +
                                p.IdClienteNavigation.ApellidoPaterno + " " +
                                p.IdClienteNavigation.ApellidoMaterno,
                PremioSharkcoins = 0,
                Puesto = 0,
                MontoPagado = 0
            }).ToList();

            var viewModel = new GanadoresViewModel
            {
                Torneo = torneo,
                Participantes = ganadores
            };

            return View("~/Views/Ganadores/Registrar.cshtml", viewModel);
        }





        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] GuardarGanadoresRequest request)
        {
            if (!TienePermiso("GanadoresTorneo", "Registrar"))
                return Forbid();

            var torneo = await _context.Torneos.FindAsync(request.IdTorneo);
            if (torneo == null) return NotFound();

            foreach (var g in request.Ganadores)
            {
                var yaExiste = await _context.GanadoresTorneos
                    .AnyAsync(gt => gt.IdTorneo == request.IdTorneo && gt.IdCliente == g.IdCliente);
                if (yaExiste) continue;

                var entity = new GanadoresTorneo
                {
                    IdTorneo = request.IdTorneo,
                    IdCliente = g.IdCliente,
                    PremioSharkcoins = g.PremioSharkcoins,
                    Puesto = g.Puesto
                };
                _context.GanadoresTorneos.Add(entity);

                var cliente = await _context.Clientes.FindAsync(g.IdCliente);
                if (cliente != null)
                {
                    // Agregar sharkcoins
                    cliente.SharkCoins = (cliente.SharkCoins ?? 0) + g.PremioSharkcoins;

                    // Calcular y agregar deuda si el monto pagado es menor a la entrada
                    if (g.MontoPagado < torneo.Entrada)
                    {
                        decimal diferencia = torneo.Entrada - g.MontoPagado;
                        cliente.Deuda = (cliente.Deuda ?? 0) + diferencia;
                    }

                    _context.Clientes.Update(cliente);
                }

                // Actualizar el monto pagado en TorneoCliente
                var torneoCliente = await _context.TorneoClientes
                    .FirstOrDefaultAsync(tc => tc.IdTorneo == request.IdTorneo && tc.IdCliente == g.IdCliente);
                if (torneoCliente != null)
                {
                    torneoCliente.Pago = g.MontoPagado;
                    _context.TorneoClientes.Update(torneoCliente);
                }
            }

            torneo.Estado = "Completado";
            _context.Torneos.Update(torneo);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }



        public class GanadorDTO
        {
            public int IdCliente { get; set; }
            public string NombreCompleto { get; set; } = null!;
            public int Puesto { get; set; }
            public decimal PremioSharkcoins { get; set; }
            public decimal MontoPagado { get; set; }
        }

        public class GanadoresViewModel
        {
            public Torneo Torneo { get; set; } = null!;
            public List<GanadorDTO> Participantes { get; set; } = new();
        }

        public class GuardarGanadoresRequest
        {
            public int IdTorneo { get; set; }
            public List<GanadorDTO> Ganadores { get; set; } = new();
        }

        public async Task<IActionResult> Index(string? cliente, string? torneo, DateTime? fechaInicio, DateTime? fechaFin, int page = 1)
        {
            if (!TienePermiso("GanadoresTorneo", "Ver"))
                return Forbid();

            int pageSize = 10;

            var query = _context.GanadoresTorneos
                .Include(gt => gt.TorneoCliente)
                    .ThenInclude(tc => tc.IdClienteNavigation)
                .Include(gt => gt.TorneoCliente)
                    .ThenInclude(tc => tc.IdTorneoNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cliente))
            {
                string filtro = cliente.ToLower();
                query = query.Where(gt =>
                    (gt.TorneoCliente.IdClienteNavigation.Nombre + " " +
                     gt.TorneoCliente.IdClienteNavigation.ApellidoPaterno + " " +
                     gt.TorneoCliente.IdClienteNavigation.ApellidoMaterno).ToLower().Contains(filtro));
            }

            if (!string.IsNullOrEmpty(torneo))
            {
                string filtroTorneo = torneo.ToLower();
                query = query.Where(gt => gt.TorneoCliente.IdTorneoNavigation.Nombre.ToLower().Contains(filtroTorneo));
            }

            if (fechaInicio.HasValue)
            {
                var fechaDesde = DateOnly.FromDateTime(fechaInicio.Value);
                query = query.Where(gt => gt.TorneoCliente.IdTorneoNavigation.Fecha >= fechaDesde);
            }

            if (fechaFin.HasValue)
            {
                var fechaHasta = DateOnly.FromDateTime(fechaFin.Value);
                query = query.Where(gt => gt.TorneoCliente.IdTorneoNavigation.Fecha <= fechaHasta);
            }

            int totalRegistros = await query.CountAsync();

            var ganadores = await query
                .OrderByDescending(gt => gt.TorneoCliente.IdTorneoNavigation.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRegistros / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.FiltroCliente = cliente;
            ViewBag.FiltroTorneo = torneo;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View("~/Views/Ganadores/Index.cshtml", ganadores);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!TienePermiso("GanadoresTorneo", "Ver"))
                return Forbid();

            if (id == null)
                return NotFound();

            var ganador = await _context.GanadoresTorneos
                .Include(gt => gt.TorneoCliente)
                    .ThenInclude(tc => tc.IdClienteNavigation)
                .Include(gt => gt.TorneoCliente)
                    .ThenInclude(tc => tc.IdTorneoNavigation)
                .FirstOrDefaultAsync(gt => gt.Id == id);

            if (ganador == null)
                return NotFound();

            return View("~/Views/Ganadores/Details.cshtml", ganador);
        }

        public async Task<IActionResult> VerGanadores(int idTorneo)
        {
            if (!TienePermiso("GanadoresTorneo", "Ver"))
                return Forbid();

            var torneo = await _context.Torneos.FindAsync(idTorneo);
            if (torneo == null) return NotFound();

            var ganadores = await _context.GanadoresTorneos
                .Include(g => g.TorneoCliente)
                    .ThenInclude(tc => tc.IdClienteNavigation)
                .Where(g => g.IdTorneo == idTorneo)
                .OrderBy(g => g.Puesto)
                .ToListAsync();

            ViewBag.Torneo = torneo;
            return View("~/Views/Ganadores/VerGanadores.cshtml", ganadores);
        }
    }
}
