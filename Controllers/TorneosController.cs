using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMDGSharksWeb.Models;

namespace ProyectoMDGSharksWeb.Controllers
{
    [Authorize]
    public class TorneosController : Controller
    {
        private readonly SharksDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string PARTICIPANTES_SESSION_KEY = "ParticipantesTorneo";

        public TorneosController(SharksDbContext context, IHttpContextAccessor httpContextAccessor)
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

        // GET: Torneos/Create
        public IActionResult Create()
        {
            if (!TienePermiso("Torneo", "Crear")) return Forbid();

            LimpiarVector();
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.IdUsuario = idUsuario;
            return View();
        }

        [HttpGet]
        public JsonResult BuscarClientes(string term)
        {
            if (!TienePermiso("Torneo", "Crear")) return Json(null);

            term ??= string.Empty;
            var clientes = _context.Clientes
                .Where(c => c.Nombre.Contains(term) || c.ApellidoPaterno.Contains(term) || c.ApellidoMaterno.Contains(term))
                .OrderBy(c => c.Nombre)
                .Select(c => new { id = c.Id, nombreCompleto = c.Nombre + " " + c.ApellidoPaterno + " " + c.ApellidoMaterno })
                .ToList();
            return Json(clientes);
        }

        [HttpPost]
        public IActionResult AgregarParticipante([FromBody] TorneoClienteDTO dto)
        {
            if (!TienePermiso("Torneo", "Crear")) return Forbid();
            if (dto == null) return BadRequest("Datos inválidos");

            var lista = ObtenerVector();
            lista.RemoveAll(x => x.IdCliente == dto.IdCliente);
            lista.Add(dto);
            GuardarVector(lista);
            return Ok();
        }

        [HttpPost]
        public IActionResult EliminarParticipante([FromBody] int idCliente)
        {
            if (!TienePermiso("Torneo", "Crear")) return Forbid();

            var lista = ObtenerVector();
            lista.RemoveAll(x => x.IdCliente == idCliente);
            GuardarVector(lista);
            return Ok();
        }

        [HttpGet]
        public IActionResult ObtenerParticipantes()
        {
            if (!TienePermiso("Torneo", "Crear")) return Json(null);

            var lista = ObtenerVector();
            var datos = lista.Select(p => new
            {
                p.IdCliente,
                ClienteNombre = _context.Clientes.Where(c => c.Id == p.IdCliente)
                    .Select(c => c.Nombre + " " + c.ApellidoPaterno + " " + c.ApellidoMaterno)
                    .FirstOrDefault(),
                p.Pago,
                p.TipoPago
            }).ToList();
            return Json(datos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarTorneo(Torneo torneo)
        {
            if (!TienePermiso("Torneo", "Crear")) return Forbid();
            if (!ModelState.IsValid) return BadRequest();

            var participantes = ObtenerVector();
            if (!participantes.Any()) return BadRequest("Debe registrar al menos un participante.");

            _context.Torneos.Add(torneo);
            await _context.SaveChangesAsync();
            foreach (var p in participantes)
            {
                var detalle = new TorneoCliente { IdTorneo = torneo.Id, IdCliente = p.IdCliente, Pago = torneo.Entrada, TipoPago = p.TipoPago };
                _context.TorneoClientes.Add(detalle);
                // Ya no calculamos deuda aquí, se hará al registrar ganadores
            }
            await _context.SaveChangesAsync();
            LimpiarVector();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CancelarTorneo()
        {
            if (!TienePermiso("Torneo", "Crear")) return Forbid();
            LimpiarVector();
            return Ok();
        }

        // GET: Torneos
        public async Task<IActionResult> Index(int page = 1, string? nombre = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            if (!TienePermiso("Torneo", "Ver")) return Forbid();

            int pageSize = 10;
            var query = _context.Torneos.AsQueryable();
            if (!string.IsNullOrEmpty(nombre)) query = query.Where(t => t.Nombre.Contains(nombre));
            if (fechaInicio.HasValue) { var fi = DateOnly.FromDateTime(fechaInicio.Value.Date); query = query.Where(t => t.Fecha >= fi); }
            if (fechaFin.HasValue) { var ff = DateOnly.FromDateTime(fechaFin.Value.Date); query = query.Where(t => t.Fecha <= ff); }

            var totalItems = await query.CountAsync();
            var torneos = await query.OrderByDescending(t => t.Fecha).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.FiltroNombre = nombre;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View(torneos);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!TienePermiso("Torneo", "Ver")) return Forbid();

            var torneo = await _context.Torneos.FirstOrDefaultAsync(t => t.Id == id);
            if (torneo == null) return NotFound();

            var participantes = await _context.TorneoClientes.Where(t => t.IdTorneo == id)
                .Include(t => t.IdClienteNavigation)
                .Select(t => new ParticipanteViewData { IdCliente = t.IdCliente, NombreCliente = t.IdClienteNavigation.Nombre + " " + t.IdClienteNavigation.ApellidoPaterno + " " + t.IdClienteNavigation.ApellidoMaterno, Pago = t.Pago, TipoPago = t.TipoPago })
                .ToListAsync();
            ViewBag.Participantes = participantes;
            return View(torneo);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!TienePermiso("Torneo", "Editar")) return Forbid();

            var torneo = await _context.Torneos.Include(t => t.TorneoClientes).ThenInclude(tc => tc.IdClienteNavigation).FirstOrDefaultAsync(t => t.Id == id);
            if (torneo == null) return NotFound();
            if (torneo.Estado == "Completado") return RedirectToAction("Index");

            var participantes = torneo.TorneoClientes.Select(tc => new TorneoClienteDTO { IdCliente = tc.IdCliente, Pago = tc.Pago, TipoPago = tc.TipoPago }).ToList();
            HttpContext.Session.SetString("ParticipantesEditTorneo", System.Text.Json.JsonSerializer.Serialize(participantes));

            ViewBag.TorneoId = torneo.Id; ViewBag.Entrada = torneo.Entrada; ViewBag.Nombre = torneo.Nombre; ViewBag.Fecha = torneo.Fecha.ToString("yyyy-MM-dd");
            return View(torneo);
        }

        [HttpGet]
        public IActionResult ObtenerParticipantesEdit()
        {
            var json = HttpContext.Session.GetString("ParticipantesEditTorneo");
            if (string.IsNullOrEmpty(json)) return Json(new List<object>());
            var lista = System.Text.Json.JsonSerializer.Deserialize<List<TorneoClienteDTO>>(json)!;
            var datos = lista.Select(p => new { IdCliente = p.IdCliente, ClienteNombre = _context.Clientes.Where(c => c.Id == p.IdCliente).Select(c => c.Nombre + " " + c.ApellidoPaterno + " " + c.ApellidoMaterno).FirstOrDefault(), Pago = p.Pago, TipoPago = p.TipoPago }).ToList();
            return Json(datos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarParticipanteEdit([FromBody] TorneoClienteDTO dto)
        {
            if (!TienePermiso("Torneo", "Editar")) return Forbid();
            if (dto == null) return BadRequest("Datos inválidos");
            var json = HttpContext.Session.GetString("ParticipantesEditTorneo");
            var lista = string.IsNullOrEmpty(json) ? new List<TorneoClienteDTO>() : System.Text.Json.JsonSerializer.Deserialize<List<TorneoClienteDTO>>(json)!;
            lista.RemoveAll(x => x.IdCliente == dto.IdCliente); lista.Add(dto);
            HttpContext.Session.SetString("ParticipantesEditTorneo", System.Text.Json.JsonSerializer.Serialize(lista));
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarParticipanteEdit([FromBody] int idCliente)
        {
            if (!TienePermiso("Torneo", "Editar")) return Forbid();
            var json = HttpContext.Session.GetString("ParticipantesEditTorneo"); if (string.IsNullOrEmpty(json)) return BadRequest("No hay participantes para editar.");
            var lista = System.Text.Json.JsonSerializer.Deserialize<List<TorneoClienteDTO>>(json)!; lista.RemoveAll(p => p.IdCliente == idCliente);
            HttpContext.Session.SetString("ParticipantesEditTorneo", System.Text.Json.JsonSerializer.Serialize(lista)); return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCambiosEdicion(int idTorneo)
        {
            if (!TienePermiso("Torneo", "Editar")) return Forbid();
            var torneo = await _context.Torneos.FindAsync(idTorneo);
            if (torneo == null) return NotFound();
            var json = HttpContext.Session.GetString("ParticipantesEditTorneo");
            if (string.IsNullOrEmpty(json)) return BadRequest("No hay participantes en sesión.");
            var nuevos = System.Text.Json.JsonSerializer.Deserialize<List<TorneoClienteDTO>>(json)!;
            var originales = await _context.TorneoClientes.Where(tc => tc.IdTorneo == idTorneo).ToListAsync();

            foreach (var original in originales.ToList())
            {
                var actualizado = nuevos.FirstOrDefault(n => n.IdCliente == original.IdCliente);
                if (actualizado == null)
                {
                    // Se eliminó el participante
                    _context.TorneoClientes.Remove(original);
                }
                else if (original.TipoPago != actualizado.TipoPago)
                {
                    // Solo actualizamos tipo de pago, el monto sigue siendo entrada
                    original.TipoPago = actualizado.TipoPago;
                    _context.TorneoClientes.Update(original);
                }
            }

            foreach (var nuevo in nuevos)
            {
                if (!originales.Any(o => o.IdCliente == nuevo.IdCliente))
                {
                    // Nuevo participante
                    var nuevoRegistro = new TorneoCliente
                    {
                        IdTorneo = idTorneo,
                        IdCliente = nuevo.IdCliente,
                        Pago = torneo.Entrada,
                        TipoPago = nuevo.TipoPago
                    };
                    _context.TorneoClientes.Add(nuevoRegistro);
                }
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("ParticipantesEditTorneo");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!TienePermiso("Torneo", "Eliminar")) return Forbid();
            var torneo = await _context.Torneos.FindAsync(id); if (torneo == null) return NotFound();
            if (torneo.Estado == "Completado") return BadRequest("No se puede eliminar un torneo completado.");
            var participantes = await _context.TorneoClientes.Where(tc => tc.IdTorneo == id).ToListAsync();
            foreach (var p in participantes)
            {
                var cliente = await _context.Clientes.FindAsync(p.IdCliente);
                if (cliente != null) { cliente.Deuda = (cliente.Deuda ?? 0) - p.Pago; if (cliente.Deuda < 0) cliente.Deuda = 0; _context.Clientes.Update(cliente); }
            }
            _context.TorneoClientes.RemoveRange(participantes); _context.Torneos.Remove(torneo);
            await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
        }

        // Utilidades de Session
        private List<TorneoClienteDTO> ObtenerVector()
        {
            var json = HttpContext.Session.GetString(PARTICIPANTES_SESSION_KEY);
            if (string.IsNullOrEmpty(json)) return new List<TorneoClienteDTO>();
            return System.Text.Json.JsonSerializer.Deserialize<List<TorneoClienteDTO>>(json)!;
        }
        private void GuardarVector(List<TorneoClienteDTO> lista)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(lista);
            HttpContext.Session.SetString(PARTICIPANTES_SESSION_KEY, json);
        }
        private void LimpiarVector()
        {
            HttpContext.Session.Remove(PARTICIPANTES_SESSION_KEY);
        }

        public class ParticipanteViewData
        {
            public int IdCliente { get; set; }
            public string NombreCliente { get; set; } = null!;
            public decimal Pago { get; set; }
            public string TipoPago { get; set; } = null!;
        }

        public class TorneoClienteDTO
        {
            public int IdCliente { get; set; }
            public decimal Pago { get; set; }
            public string TipoPago { get; set; } = null!;
        }
    }
}
