import { Cotizacion } from '../entities/cotizacion.entity';

export interface ICotizacionRepository {
  guardar(cotizacion: Cotizacion): Promise<void>;
  buscarTodas(): Promise<Cotizacion[]>;
  buscarPorId(id: string): Promise<Cotizacion | null>;
}

export const COTIZACION_REPOSITORY = Symbol('CotizacionRepository');