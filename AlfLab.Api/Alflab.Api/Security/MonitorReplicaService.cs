using Microsoft.Extensions.Hosting;
using MySqlConnector;
using System.Threading;

namespace AlfLab.Api.Security
{
    public class MonitorReplicaService : BackgroundService
    {
        private readonly EstadoSistema _estado;
        private readonly string _replicaConnectionString;

        public MonitorReplicaService(EstadoSistema estado)
        {
            _estado = estado;
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "root";
            // Apunta al nombre del servicio en Docker y al puerto interno 3306
            _replicaConnectionString = $"Server=db-replica;Port=3306;Uid=root;Pwd={dbPassword};";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🛡️ Robot Vigía de Réplica iniciado en segundo plano.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Intentamos conectar a la réplica
                    using var connection = new MySqlConnection(_replicaConnectionString);
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await connection.OpenAsync(cts.Token);
                    
                    // Si logró conectar y el semáforo estaba en rojo, lo CURA (Lo pone en verde)
                    if (!_estado.ReplicaEstaActiva) 
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✅ [AUTO-RECOVERY] La réplica ha vuelto. Semáforo en verde. Operaciones restauradas.");
                        Console.ResetColor();
                        _estado.ReplicaEstaActiva = true;
                    }
                }
                catch
                {
                    // Si falla la conexión y el semáforo estaba verde, enciende la alarma
                    if (_estado.ReplicaEstaActiva) 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("🚨 [ALERTA] Réplica caída. Semáforo en rojo. Activando protocolo de protección...");
                        Console.ResetColor();
                        _estado.ReplicaEstaActiva = false;
                    }
                }

                // El robot descansa 3 segundos y vuelve a revisar
                await Task.Delay(3000, stoppingToken); 
            }
        }
    }
}