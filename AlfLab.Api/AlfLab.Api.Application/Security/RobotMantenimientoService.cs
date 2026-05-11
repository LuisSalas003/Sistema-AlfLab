using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using AlfLab.Api.Application.Interfaces; 

namespace AlfLab.Api.Application.Services 
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
            _logger.LogInformation("🤖 Robot de segundo plano iniciado en .NET.");

            // El ciclo de vida infinito del robot
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // CRÍTICO: Creamos un "Scope" temporal para poder usar los repositorios
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _logger.LogInformation("⚙️ Ejecutando tarea automática en segundo plano...");
                        // Aquí puedes inyectar cualquier servicio que necesites, por ejemplo:
                
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🚨 Error en el robot de segundo plano.");
                }

                // El robot se va a dormir 1 minuto (o lo que tú decidas) y vuelve a empezar
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}