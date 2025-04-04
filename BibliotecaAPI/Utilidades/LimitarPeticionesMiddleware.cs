using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BibliotecaAPI.Utilidades
{
    public static class LimitarPeticionesMiddlewareExtension
    {
        public static IApplicationBuilder UseLimitarPeticiones(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LimitarPeticionesMiddleware>();
        }
    }

    public class LimitarPeticionesMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptionsMonitor<LimitarPeticionesDTO> optionsMonitorLimitarPeticiones;

        public LimitarPeticionesMiddleware(RequestDelegate next, IOptionsMonitor<LimitarPeticionesDTO> optionsMonitorLimitarPeticiones)
        {
            this.next = next;
            this.optionsMonitorLimitarPeticiones = optionsMonitorLimitarPeticiones;
        }

        public async Task InvokeAsync(HttpContext httpContext, ApplicationDbContext context)
        {
            var limitarPeticionesDTO = optionsMonitorLimitarPeticiones.CurrentValue;

            var llaveStringValues = httpContext.Request.Headers["X-Api-Key"];

            if (llaveStringValues.Count == 0)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Debe proveer la llave en la cabecera X-Api-Key");
                return;
            }

            if (llaveStringValues.Count > 1)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Solo una llave debe de estar presente");
                return;
            }

            var llave = llaveStringValues[0];

            var llaveDB = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Llave == llave);

            if (llaveDB is null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave no existe");
                return;
            }

            if (!llaveDB.Activa)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave se encuentra inactiva");
                return;
            }

            if (llaveDB.TipoLLave == Entidades.TipoLlave.Gratuita)
            {
                var hoy = DateTime.UtcNow.Date;
                var cantidadPeticionesRealizadasHoy = await context.Peticiones.CountAsync(x => x.LlaveId == llaveDB.Id && x.FechaPeticion.Date >= hoy);

                if (limitarPeticionesDTO.PeticionesPorDiaGratuito <= cantidadPeticionesRealizadasHoy)
                {
                    httpContext.Response.StatusCode = 429; // Too many requests
                    await httpContext.Response.WriteAsync("Ha excedido el limite de peticiones por dia. Si desea realizar mas peticiones, actualice su suscripcion a una cuenta profesional");
                    return;
                }
            }


            var peticion = new Peticion() { LlaveId = llaveDB.Id, FechaPeticion = DateTime.UtcNow };

            context.Add(peticion);
            await context.SaveChangesAsync();

            await next(httpContext);


        }
    }
}
