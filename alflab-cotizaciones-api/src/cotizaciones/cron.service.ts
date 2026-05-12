import { Injectable, Logger } from '@nestjs/common';
import { Cron, CronExpression } from '@nestjs/schedule';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository, LessThan } from 'typeorm';
import { CotizacionOrmEntity } from './infrastructure/database/entities/cotizacion.orm-entity'; 
import { ReplicaMonitorService } from '../seguridad/replica-monitor.service'; 

@Injectable()
export class LimpiadorCotizacionesService {
  private readonly logger = new Logger(LimpiadorCotizacionesService.name);

  constructor(
    @InjectRepository(CotizacionOrmEntity)
    private readonly cotizacionRepository: Repository<CotizacionOrmEntity>,
    // 👇 2. Inyectamos el Vigía en el constructor
    private readonly monitor: ReplicaMonitorService,
  ) {}

  @Cron(CronExpression.EVERY_10_SECONDS) 
  async limpiarCotizacionesVencidas() {
    this.logger.log('Iniciando escaneo de cotizaciones expiradas...');
    
    // 👇 3. EL CANDADO: Si el semáforo está en rojo, abortamos la limpieza silenciosamente
    if (!this.monitor.isReplicaActive) {
      this.logger.warn('⏳ Escaneo pausado: Esperando a que la base de datos se recupere.');
      return; 
    }

    const fechaLimite = new Date();
    fechaLimite.setDate(fechaLimite.getDate() - 15);

    try {
      // Usamos el repositorio del ORM
      const resultado = await this.cotizacionRepository.update(
        { estado: 'ENVIADA', fechaCreacion: LessThan(fechaLimite) },
        { estado: 'RECHAZADA' }
      );

      const filasAfectadas = resultado.affected ?? 0;

      if (filasAfectadas > 0) {
        this.logger.log(`¡Limpieza exitosa! Se rechazaron ${filasAfectadas} cotizaciones por caducidad.`);
      } else {
        this.logger.log('Escaneo terminado. Ninguna cotización vieja encontrada.');
      }
      
    } catch (error) {
      this.logger.error('Hubo un problema al intentar limpiar la base de datos', error);
    }
  }
}