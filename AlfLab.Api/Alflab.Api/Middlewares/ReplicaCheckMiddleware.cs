using Microsoft.AspNetCore.Http; // u otros usings que ya tengas
using System.Threading.Tasks;
using AlfLab.Api.Security; // Para acceder al EstadoSistema compartido

namespace AlfLab.Api.Middlewares
{
    public class ReplicaCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly EstadoSistema _estado;

        // Inyectamos el semáforo
        public ReplicaCheckMiddleware(RequestDelegate next, EstadoSistema estado)
        {
            _next = next;
            _estado = estado;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Verificación ultra rápida en RAM (No afecta el rendimiento)
            if (!_estado.ReplicaEstaActiva)
            {
                // Devolvemos un 200 OK (Para que no salte como "Error" en el frontend)
                // Pero con un mensaje estructurado indicando que se está solucionando
                context.Response.StatusCode = StatusCodes.Status200OK; 
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"success\": false, \"message\": \"El sistema está realizando rutinas de auto-recuperación y sincronización en segundo plano. Por favor, intente su operación en unos segundos.\"}");
                return; // Bloquea Selects, Inserts, Deletes, absolutamente todas las operaciones.
            }

            // Si el semáforo está en verde, pasa al instante
            await _next(context);
        }
    }
}