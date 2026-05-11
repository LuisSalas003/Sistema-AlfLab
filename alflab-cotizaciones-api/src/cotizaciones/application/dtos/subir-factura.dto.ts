import { ApiProperty } from '@nestjs/swagger';

export class SubirFacturaDto {
  @ApiProperty({ example: 'comprobante_pago.pdf', description: 'Nombre del archivo adjunto' })
  nombreArchivo!: string;

  @ApiProperty({ example: 'JVBERi0xLjQK...', description: 'Contenido simulado en Base64' })
  contenidoBase64!: string;
}