using MySqlConnector;
using System.Threading; // 👇 Añadido para manejar el tiempo límite

namespace AlfLab.Api.Middlewares
{
    public class ReplicaCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _replicaConnectionString;

        public ReplicaCheckMiddleware(RequestDelegate next)
        {
            _next = next;
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "root";
            _replicaConnectionString = $"Server=127.0.0.1;Port=3309;Uid=root;Pwd={dbPassword};";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post || 
                context.Request.Method == HttpMethods.Put || 
                context.Request.Method == HttpMethods.Delete)
            {
                try
                {
                    using var connection = new MySqlConnection(_replicaConnectionString);
                    
                    // 👇 AQUÍ ESTÁ LA MODIFICACIÓN EXACTA 👇
                    // Creamos un cronómetro de 2 segundos. Si la réplica no responde 
                    // en ese tiempo, el guardia asume que está muerta y corta el proceso.
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await connection.OpenAsync(cts.Token); 
                    // 👆 FIN DE LA MODIFICACIÓN 👆
                }
                catch (Exception ex) 
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Replica Check Failed]: {ex.Message}");
                    Console.ResetColor();

                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Sistema en modo de solo lectura. La base de datos réplica no está disponible o está en mantenimiento.\"}");
                    return; 
                }
            }

            await _next(context);
        }
    }
}