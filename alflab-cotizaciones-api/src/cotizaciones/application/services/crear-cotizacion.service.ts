import { Injectable, Inject } from '@nestjs/common';
import { Cotizacion } from '../../domain/entities/cotizacion.entity';
import * as cotizacionRepository from '../../domain/interfaces/cotizacion.repository';

@Injectable()
export class CrearCotizacionService {
  constructor(
    @Inject(cotizacionRepository.COTIZACION_REPOSITORY)
    private readonly cotizacionRepository: cotizacionRepository.ICotizacionRepository,
  ) {}

  async ejecutar(clienteId: string, total: number): Promise<Cotizacion> {
    const id = crypto.randomUUID();
    const folio = `COT-${Math.floor(Math.random() * 10000)}`;
    
    // Creamos la entidad
    const nuevaCotizacion = new Cotizacion(id, folio, clienteId, total, 'BORRADOR', new Date(), new Date());
    
    // Guardamos usando el puerto
    await this.cotizacionRepository.guardar(nuevaCotizacion);
    
    return nuevaCotizacion;
  }
}