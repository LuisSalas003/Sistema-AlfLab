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
            _logger.LogInformation("🤖 Robot de mantenimiento iniciado. Patrullando cada 5 minutos...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // El robot ahora solo busca usuarios bloqueados cuyo tiempo de castigo ya expiró
                    var usuariosADesbloquear = await context.Usuarios
                        .Where(u => u.BloqueadoHasta != null && u.BloqueadoHasta <= DateTime.UtcNow)
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

                // El robot se va a dormir 5 minutos antes de volver a revisar
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}