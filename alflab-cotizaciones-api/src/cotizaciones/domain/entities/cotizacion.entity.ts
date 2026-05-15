export type EstadoCotizacion = 'BORRADOR' | 'ENVIADA' | 'ACEPTADA' | 'RECHAZADA';

export class Cotizacion {
  constructor(
    public readonly id: string,
    public readonly folio: string,
    public readonly clienteId: string,
    public total: number,
    public estado: EstadoCotizacion,
    public readonly fechaCreacion: Date,
    public fechaActualizacion: Date,
  ) {}

  // Este es el método que Node.js no está encontrando
  marcarComoEnviada(): void {
    if (this.estado !== 'BORRADOR') {
      throw new Error('Solo cotizaciones en BORRADOR pueden enviarse.');
    }
    this.estado = 'ENVIADA';
    this.fechaActualizacion = new Date();
  }
}