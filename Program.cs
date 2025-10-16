using ProyectoMDGSharksWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoMDGSharksWeb.Middleware;
using ProyectoMDGSharksWeb.Middleware;
var builder = WebApplication.CreateBuilder(args);

// 💡 Agrega el contexto SharksDbContext con la cadena de conexión
builder.Services.AddDbContext<SharksDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 💡 Configura autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";       // Ruta para el login
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/MostrarAccesoDenegado";// Ruta para logout
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Tiempo de expiración de la cookie
    });

// 💡 Habilita Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Agrega servicios MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
 // 👈 Importa el namespace

// ...

  // 👈 Agrega esto

app.UseRouting();

// 💡 Activa autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<AccesoDenegadoMiddleware>();
// 💡 Usa Session antes de MapControllerRoute
app.UseSession();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
