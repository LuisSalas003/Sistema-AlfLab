using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlfLab.Api.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AlfLab.Api.Security
{
    public class RobotMantenimientoService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RobotMantenimientoService> _logger;

        public RobotMantenimientoService(IServiceProvider serviceProvider, ILogger<RobotMantenimientoService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Cambiamos el mensaje para que sepas que estás en "Modo Pruebas"
            _logger.LogInformation("🤖 Robot de mantenimiento iniciado. Patrullando en MODO QA (cada 10 segundos)...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // 👇 HACK DE QA: Quitamos la validación de la hora para que perdone inmediatamente
                    var usuariosADesbloquear = await context.Usuarios
                        .Where(u => u.BloqueadoHasta != null) 
                        .ToListAsync(stoppingToken);

                    if (usuariosADesbloquear.Any())
                    {
                        foreach (var user in usuariosADesbloquear)
                        {
                            user.BloqueadoHasta = null;
                            user.IntentosFallidos = 0;
                            _logger.LogInformation("✅ Robot: Usuario {CorreoUsuario} ha sido desbloqueado.", user.Correo);
                        }
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }

                // 👇 HACK DE QA: El robot se va a dormir solo 10 segundos
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}