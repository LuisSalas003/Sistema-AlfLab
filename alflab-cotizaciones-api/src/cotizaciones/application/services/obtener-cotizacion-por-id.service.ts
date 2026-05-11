import { Injectable, Inject, NotFoundException } from '@nestjs/common';
import { Cotizacion } from '../../domain/entities/cotizacion.entity';
import type { ICotizacionRepository } from '../../domain/interfaces/cotizacion.repository';
import { COTIZACION_REPOSITORY } from '../../domain/interfaces/cotizacion.repository';

@Injectable()
export class ObtenerCotizacionPorIdService {
  constructor(
    @Inject(COTIZACION_REPOSITORY)
    private readonly repository: ICotizacionRepository,
  ) {}

  async ejecutar(id: string): Promise<Cotizacion> {
    const cotizacion = await this.repository.buscarPorId(id);
    if (!cotizacion) {
      throw new NotFoundException(`La cotización con ID ${id} no existe.`);
    }
    return cotizacion;
  }
}