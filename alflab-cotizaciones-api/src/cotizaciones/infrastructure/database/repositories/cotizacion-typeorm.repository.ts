import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { ICotizacionRepository } from '../../../domain/interfaces/cotizacion.repository'; 
import { Cotizacion, EstadoCotizacion } from '../../../domain/entities/cotizacion.entity';
import { CotizacionOrmEntity } from '../entities/cotizacion.orm-entity';

@Injectable()
export class CotizacionTypeOrmRepository implements ICotizacionRepository {
  constructor(
    @InjectRepository(CotizacionOrmEntity)
    private readonly ormRepository: Repository<CotizacionOrmEntity>,
  ) {}

  // 👇 ESTE ES EL SALVAVIDAS: Convierte el JSON de MySQL a tu Clase Pura 👇
  private toDomain(orm: CotizacionOrmEntity): Cotizacion {
    return new Cotizacion(
      orm.id, 
      orm.folio, 
      orm.clienteId, 
      Number(orm.total), 
      orm.estado as EstadoCotizacion, 
      orm.fechaCreacion, 
      orm.fechaActualizacion
    );
  }

  async guardar(cotizacion: Cotizacion): Promise<void> {
    const ormEntity = this.ormRepository.create({
      id: cotizacion.id, folio: cotizacion.folio, clienteId: cotizacion.clienteId, 
      total: cotizacion.total, estado: cotizacion.estado,
    });
    await this.ormRepository.save(ormEntity);
  }

  async buscarTodas(): Promise<Cotizacion[]> {
    const ormEntities = await this.ormRepository.find();
    return ormEntities.map(orm => this.toDomain(orm)); // Usamos el salvavidas
  }

  async buscarPorId(id: string): Promise<Cotizacion | null> {
    const ormEntity = await this.ormRepository.findOne({ where: { id } });
    if (!ormEntity) return null;
    
    return this.toDomain(ormEntity); // Usamos el salvavidas
  }
}