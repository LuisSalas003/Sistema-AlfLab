import { Injectable, Logger } from '@nestjs/common';
import { Cron, CronExpression } from '@nestjs/schedule';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CotizacionOrmEntity } from '../../infrastructure/database/entities/cotizacion.orm-entity';

@Injectable()
export class CotizacionesCronService {
  private readonly logger = new Logger(CotizacionesCronService.name);

  constructor(
    @InjectRepository(CotizacionOrmEntity)
    private readonly cotizacionRepo: Repository<CotizacionOrmEntity>,
  ) {}

  // 🤖 TAREA 1: Simulador de facturación (El que acabamos de hacer)
  @Cron(CronExpression.EVERY_MINUTE)
  async procesarFacturasAutomaticas() {
    this.logger.debug('🤖 Iniciando simulación de facturación...');
    const cotizacionesPendientes = await this.cotizacionRepo.find({ where: { estado: 'BORRADOR' } });

    if (cotizacionesPendientes.length === 0) return;

    for (const cotizacion of cotizacionesPendientes) {
      cotizacion.estado = 'FACTURADA'; 
      await this.cotizacionRepo.save(cotizacion);
      this.logger.log(`✅ Cotización ${cotizacion.folio || cotizacion.id} facturada.`);
    }
  }

  // 🧹 TAREA 2: El limpiador que ya tenías (Ajusta la lógica a lo que tenías programado)
  @Cron(CronExpression.EVERY_DAY_AT_MIDNIGHT) // O el tiempo que le tuvieras asignado
  async limpiarCotizacionesViejas() {
    this.logger.debug('🧹 Ejecutando limpieza de cotizaciones antiguas...');
    // Pega aquí la lógica que tenías en tu LimpiadorCotizacionesService
  }
}