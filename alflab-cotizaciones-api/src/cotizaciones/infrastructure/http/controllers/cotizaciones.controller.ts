import { Controller, Post, Body, Param, BadRequestException, Req, UseGuards, Get, Patch } from '@nestjs/common';
import { CrearCotizacionService } from '../../../application/services/crear-cotizacion.service';
import { ObtenerCotizacionesService } from '../../../application/services/obtener-cotizaciones.service';
import { ObtenerCotizacionPorIdService } from '../../../application/services/obtener-cotizacion-por-id.service';
import { EnviarCotizacionService } from '../../../application/services/enviar-cotizacion.service';
import { CrearCotizacionDto } from '../dtos/crear-cotizacion.dto';
import { AuthGuard } from '@nestjs/passport/dist/auth.guard';
import { SubirFacturaDto } from '../../../application/dtos/subir-factura.dto';
import { SecurityLoggerService } from '../../../application/services/security-logger.service';
import type { Request } from 'express';

@Controller('cotizaciones')
export class CotizacionesController {
  constructor(
    private readonly crearService: CrearCotizacionService,
    private readonly obtenerTodasService: ObtenerCotizacionesService,
    private readonly obtenerPorIdService: ObtenerCotizacionPorIdService,
    private readonly enviarService: EnviarCotizacionService,
    private readonly securityLogger: SecurityLoggerService
  ) {}

  @Post()
  async crear(@Body() dto: CrearCotizacionDto) {
    const data = await this.crearService.ejecutar(dto.clienteId, dto.total);
    return { mensaje: 'Cotización creada', data };
  }

  @UseGuards(AuthGuard('jwt')) // <-- ¡ESTO CIERRA LA PUERTA!

  @Get()
  async obtenerTodas() {
    return this.obtenerTodasService.obtenerTodasLasCotizaciones();
  }

  @Get(':id')
  async obtenerPorId(@Param('id') id: string) {
    return this.obtenerPorIdService.ejecutar(id);
  }

  @Patch(':id/enviar')
  async enviar(@Param('id') id: string) {
    await this.enviarService.ejecutar(id);
    return { mensaje: `Cotización ${id} marcada como ENVIADA exitosamente.` };
  }
@Post(':id/facturas')
  async subirFacturaSimulada(
    @Param('id') id: string, 
    @Body() body: SubirFacturaDto,
    @Req() req: Request 
  ) {
    // Escudo anti-crasheo de TypeScript: Si mandan un JSON vacío, lo rechazamos directo
    if (!body?.nombreArchivo) {
      throw new BadRequestException('Falta el nombre del archivo en la petición.');
    }

    // El signo de interrogación (?) previene errores de undefined
    const extension = body.nombreArchivo.split('.').pop()?.toLowerCase();
    const extensionesPermitidas = ['pdf', 'xml', 'jpg', 'png'];

    // EL ESCUDO ANTI-HACKERS
    if (!extension || !extensionesPermitidas.includes(extension)) {
      
      // Tomamos la IP (Express moderno usa socket en vez de connection)
      const ip = req.ip || req.socket.remoteAddress || 'IP Desconocida';
      
      // Dejamos la evidencia física para el maestro
      this.securityLogger.registrarAtaque(
        'Intento de Subida de Archivo Malicioso (RCE)',
        ip,
        `POST /api/cotizaciones/${id}/facturas`,
        `Se bloqueó el archivo: ${body.nombreArchivo}`
      );

      // Pateamos al atacante
      throw new BadRequestException(`Ataque mitigado: El tipo de archivo .${extension} no está permitido. Solo se aceptan facturas.`);
    }

    // SI EL CAMINO ES FELIZ (Happy Path):
    return {
      mensaje: 'Factura subida correctamente a la cotización.',
      archivo: body.nombreArchivo
    };
  }

}
