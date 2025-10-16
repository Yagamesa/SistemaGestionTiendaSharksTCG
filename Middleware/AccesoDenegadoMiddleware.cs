namespace ProyectoMDGSharksWeb.Middleware
{
    public class AccesoDenegadoMiddleware
    {
        private readonly RequestDelegate _next;

        public AccesoDenegadoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 403)
            {
                context.Response.Redirect("/Home/MostrarAccesoDenegado");
            }
        }
    }
}
