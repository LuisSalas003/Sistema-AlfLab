using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlfLab.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Dejamos que la petición siga su camino normal
                await _next(httpContext);

                // Si alguien manda un "Bad Request" (400)
                if (httpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString();
                    // SOLUCIÓN SONARLINT 1: Usamos llaves {} y pasamos las variables como parámetros
                    _logger.LogWarning("⚠️ Posible intento de manipulación de datos (DTO inválido) desde IP: {Ip} hacia {Path}", ip, httpContext.Request.Path);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // SOLUCIÓN SONARLINT 2 y 3: Pasamos la excepción (ex) como primer parámetro y usamos llaves
                _logger.LogWarning(ex, "🔒 Bloqueo de seguridad activado: {Mensaje}", ex.Message);
                
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new { mensaje = ex.Message }));
            }
            catch (Exception ex)
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString();
                
                // SOLUCIÓN SONARLINT 4: Usamos llaves {} en lugar del signo de dólar
                _logger.LogError(ex, "🚨 EXCEPCIÓN CRÍTICA - Ruta: {Path} | IP: {Ip}", httpContext.Request.Path, ip);
                
                await HandleExceptionAsync(httpContext);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new 
            {
                mensaje = "Ocurrió un error interno. El evento ha sido registrado para auditoría."
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}