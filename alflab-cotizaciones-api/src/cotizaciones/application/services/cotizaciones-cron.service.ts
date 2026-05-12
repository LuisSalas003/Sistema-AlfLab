import { Injectable, Logger } from '@nestjs/common';
import { Cron, CronExpression } from '@nestjs/schedule';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CotizacionOrmEntity } from '../../infrastructure/database/entities/cotizacion.orm-entity';
// 👇 1. Importamos el Vigía (Verifica que los '../' sean correctos hacia tu carpeta de seguridad)
import { ReplicaMonitorService } from '../../../seguridad/replica-monitor.service';

@Injectable()
export class CotizacionesCronService {
  private readonly logger = new Logger(CotizacionesCronService.name);

  constructor(
    @InjectRepository(CotizacionOrmEntity)
    private readonly cotizacionRepo: Repository<CotizacionOrmEntity>,
    // 👇 2. Inyectamos el Vigía en el constructor
    private readonly monitor: ReplicaMonitorService,
  ) {}

  // 🤖 TAREA 1: Simulador de facturación
  @Cron(CronExpression.EVERY_MINUTE)
  async procesarFacturasAutomaticas() {
    this.logger.debug('🤖 Iniciando simulación de facturación...');
    
    // 👇 3. EL CANDADO: Protege la facturación
    if (!this.monitor.isReplicaActive) {
      this.logger.warn('⏳ Facturación pausada: Esperando a que la base de datos se recupere.');
      return; 
    }

    try {
      const cotizacionesPendientes = await this.cotizacionRepo.find({ where: { estado: 'BORRADOR' } });

      if (cotizacionesPendientes.length === 0) return;

      for (const cotizacion of cotizacionesPendientes) {
        cotizacion.estado = 'FACTURADA'; 
        await this.cotizacionRepo.save(cotizacion);
        this.logger.log(`✅ Cotización ${cotizacion.folio || cotizacion.id} facturada.`);
      }
    } catch (error: any) {
      this.logger.error(`Error en facturación: ${error.message}`);
    }
  }

  // 🧹 TAREA 2: El limpiador
  @Cron(CronExpression.EVERY_DAY_AT_MIDNIGHT) 
  async limpiarCotizacionesViejas() {
    this.logger.debug('🧹 Ejecutando limpieza de cotizaciones antiguas...');

    // 👇 4. EL CANDADO: Protege la limpieza también
    if (!this.monitor.isReplicaActive) {
      this.logger.warn('⏳ Limpieza pausada: Esperando a que la base de datos se recupere.');
      return; 
    }

    // Pega aquí la lógica que tenías en tu LimpiadorCotizacionesService
  }
}