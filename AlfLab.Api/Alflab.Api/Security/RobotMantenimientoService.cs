using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlfLab.Api.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            // 👇 ESTA ES LA PAUSA SALVAVIDAS PARA DOCKER Y EF CORE
            _logger.LogInformation("⏳ Robot Vigía: Esperando 10 segundos para que la base de datos se construya...");
            await Task.Delay(10000, stoppingToken); 

            // Cambiamos el mensaje para que sepas que estás en "Modo Pruebas"
            _logger.LogInformation("🤖 Robot de mantenimiento iniciado. Patrullando en MODO QA (cada 60 segundos)...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // HACK DE QA: Quitamos la validación de la hora para que perdone inmediatamente
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

                // El robot se va a dormir 60 segundos antes de la siguiente patrulla
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}