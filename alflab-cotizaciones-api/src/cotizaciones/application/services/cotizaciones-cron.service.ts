import { Injectable, Logger } from '@nestjs/common';
import { Cron, CronExpression } from '@nestjs/schedule';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CotizacionOrmEntity } from '../../infrastructure/database/entities/cotizacion.orm-entity';
import { ReplicaMonitorService } from '../../../seguridad/replica-monitor.service';

@Injectable()
export class CotizacionesCronService {
  private readonly logger = new Logger(CotizacionesCronService.name);

  constructor(
    @InjectRepository(CotizacionOrmEntity)
    private readonly cotizacionRepo: Repository<CotizacionOrmEntity>,
    private readonly monitor: ReplicaMonitorService,
  ) {}

  // 🤖 TAREA 1: Robot de Aceptación de Cotizaciones
  @Cron(CronExpression.EVERY_MINUTE)
  async procesarCotizacionesParaAceptar() {
    this.logger.debug('🤖 [Robot] Iniciando búsqueda de cotizaciones pendientes...');
    
    // 👇 EL CANDADO: Protege la base de datos si la réplica está caída
    if (!this.monitor.isReplicaActive) {
      this.logger.warn('⏳ [Robot] Pausado: Esperando a que la base de datos se recupere.');
      return; 
    }

    try {
      // Buscamos las cotizaciones iniciales
      const cotizacionesPendientes = await this.cotizacionRepo.find({ 
        where: { estado: 'BORRADOR' } 
      });

      // Si no hay nada que hacer, salimos silenciosamente
      if (cotizacionesPendientes.length === 0) return;

      this.logger.log(`⚙️ Encontradas ${cotizacionesPendientes.length} cotizaciones para procesar.`);

      // Procesamos y cambiamos el estado
      for (const cotizacion of cotizacionesPendientes) {
        cotizacion.estado = 'ACEPTADA'; 
        // Actualizamos la fecha de modificación para auditoría
        cotizacion.fechaActualizacion = new Date();
        
        await this.cotizacionRepo.save(cotizacion);
        this.logger.log(`✅ Cotización [${cotizacion.folio || cotizacion.id}] ha cambiado a estado ACEPTADA.`);
      }
    } catch (error: any) {
      this.logger.error(`❌ Error crítico en el robot de aceptación: ${error.message}`);
    }
  }

  // 🧹 TAREA 2: El limpiador
  @Cron(CronExpression.EVERY_DAY_AT_MIDNIGHT) 
  async limpiarCotizacionesViejas() {
    this.logger.debug('🧹 Ejecutando limpieza de cotizaciones antiguas...');

    // 👇 EL CANDADO: Protege la limpieza
    if (!this.monitor.isReplicaActive) {
      this.logger.warn('⏳ [Robot Limpiador] Pausado: Esperando recuperación de BD.');
      return; 
    }

    // Aquí va tu lógica del LimpiadorCotizacionesService
  }
}