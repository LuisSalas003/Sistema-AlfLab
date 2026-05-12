import { Injectable, Logger } from '@nestjs/common';
import { Cron, CronExpression } from '@nestjs/schedule';
import * as mysql from 'mysql2/promise';

@Injectable()
export class ReplicaMonitorService {
  private readonly logger = new Logger(ReplicaMonitorService.name);
  
  // 🟢 Este es el Semáforo en RAM
  public isReplicaActive: boolean = true; 

  @Cron(CronExpression.EVERY_5_SECONDS)
  async checkReplicaHealth() {
    try {
      const connection = await mysql.createConnection({
        host: process.env.DB_HOST === '127.0.0.1' ? '127.0.0.1' : 'db-replica', // Habla con el contenedor
        port: 3306, // Puerto interno de Docker 
        user: 'root',
        // 👇 Corrección 1: Quitamos el string en duro. Ahora confía 100% en el .env
        password: process.env.DB_PASSWORD, 
        connectTimeout: 2000 
      });
      
      await connection.ping();
      await connection.end();

      if (!this.isReplicaActive) {
        this.logger.log('✅ [AUTO-RECOVERY] La réplica ha vuelto. Semáforo en verde. Operaciones restauradas.');
        this.isReplicaActive = true;
      }
    } catch (error: any) { // 👈 Tipamos el error como 'any' o 'Error'
      
      // 👇 Corrección 2: Le mostramos a SonarLint que sí estamos registrando el motivo del error
      this.logger.debug(`Fallo de conexión detectado en réplica: ${error.message}`);

      if (this.isReplicaActive) {
        this.logger.error('🚨 [ALERTA] Réplica caída. Semáforo en rojo. Activando protección de solo lectura...');
        this.isReplicaActive = false;
      }
    }
  }
}