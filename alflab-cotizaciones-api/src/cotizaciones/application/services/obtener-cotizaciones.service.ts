import { Injectable, Logger } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';

// 👇 1. IMPORTANTE: Traemos la entidad ORM, no la de dominio
import { CotizacionOrmEntity } from '../../infrastructure/database/entities/cotizacion.orm-entity';

@Injectable()
export class ObtenerCotizacionesService {
  // Creamos un Logger rojo para que el maestro vea la alarma en la terminal
  private readonly logger = new Logger('CircuitBreaker');

  constructor(
    // 👇 2. IMPORTANTE: Usamos la entidad ORM para que NestJS la encuentre
    @InjectRepository(CotizacionOrmEntity)
    private readonly cotizacionRepo: Repository<CotizacionOrmEntity>,
  ) {}

  async obtenerTodasLasCotizaciones() {
    try {
      // 1. HAPPY PATH (CQRS): 
      // TypeORM mandará esta consulta automáticamente a la Réplica (Puerto 3307)
      return await this.cotizacionRepo.find();
      
    } catch (error) {
      // 2. SAD PATH (Fallback de Emergencia): 
      // Si el maestro apaga el contenedor de la réplica, entramos aquí en lugar de crashear.
      
      this.logger.error('⚠️ [SAD PATH] La base de datos Réplica no responde (Posible caída).');
      this.logger.error(`Error: ${(error as Error).message}`);
      this.logger.warn('🔄 Activando Cortocircuito: Redirigiendo tráfico de lectura hacia el nodo Master de forma temporal...');
      
      // EL TRUCO: Al usar un "transaction", TypeORM se ve obligado a conectarse al nodo Master 
      // para garantizar la consistencia de los datos. ¡El usuario final ni se entera de la falla!
      return await this.cotizacionRepo.manager.transaction(async (transactionalEntityManager) => {
         // 👇 3. IMPORTANTE: Usamos la entidad ORM aquí también
         return await transactionalEntityManager.find(CotizacionOrmEntity);
      });
    }
  }
}