import { Injectable, Logger } from '@nestjs/common';
import { Cron, CronExpression } from '@nestjs/schedule';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository, LessThan } from 'typeorm';
// ¡IMPORTANTE! Ahora importamos la entidad ORM, no la de dominio
import { CotizacionOrmEntity } from './infrastructure/database/entities/cotizacion.orm-entity'; 

@Injectable()
export class LimpiadorCotizacionesService {
  private readonly logger = new Logger(LimpiadorCotizacionesService.name);

  constructor(
    // Pedimos el repositorio exacto que registraste en el módulo
    @InjectRepository(CotizacionOrmEntity)
    private readonly cotizacionRepository: Repository<CotizacionOrmEntity>,
  ) {}

  @Cron(CronExpression.EVERY_10_SECONDS) 
  async limpiarCotizacionesVencidas() {
    this.logger.log('Iniciando escaneo de cotizaciones expiradas...');
    
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