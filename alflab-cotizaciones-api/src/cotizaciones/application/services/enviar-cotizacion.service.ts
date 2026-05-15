import { Injectable, Inject } from '@nestjs/common';
import * as cotizacionRepository from '../../domain/interfaces/cotizacion.repository';
import { ObtenerCotizacionPorIdService } from './obtener-cotizacion-por-id.service';

@Injectable()
export class EnviarCotizacionService {
  constructor(
    @Inject(cotizacionRepository.COTIZACION_REPOSITORY)
    private readonly repository: cotizacionRepository.ICotizacionRepository,
    private readonly obtenerPorId: ObtenerCotizacionPorIdService,
  ) {}

  async ejecutar(id: string): Promise<void> {
    const cotizacion = await this.obtenerPorId.ejecutar(id);
    
    cotizacion.marcarComoEnviada(); 
    await this.repository.guardar(cotizacion);
  }
}