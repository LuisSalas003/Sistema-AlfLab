import { ExceptionFilter, Catch, ArgumentsHost, HttpException, Injectable } from '@nestjs/common';
import { Request, Response } from 'express';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { AuditoriaOrmEntity } from '../cotizaciones/infrastructure/database/entities/auditoria.orn-entity';

@Catch(HttpException)
@Injectable()
export class SecurityAuditFilter implements ExceptionFilter {
  constructor(
    @InjectRepository(AuditoriaOrmEntity)
    private readonly auditoriaRepo: Repository<AuditoriaOrmEntity>,
  ) {}

  async catch(exception: HttpException, host: ArgumentsHost) {
    const ctx = host.switchToHttp();
    const response = ctx.getResponse<Response>();
    const request = ctx.getRequest<Request>();
    const status = exception.getStatus();

    if (status >= 400 && status < 500) {
      const log = this.auditoriaRepo.create({
        ip: request.ip || request.socket.remoteAddress || 'IP_DESCONOCIDA',
        ruta: request.url,
        metodo: request.method,
        payload: JSON.stringify(request.body || {}), 
        motivoRechazo: JSON.stringify(exception.getResponse()),
      });

      this.auditoriaRepo.save(log).catch(err => console.error('Error al guardar auditoría', err));
    }

    response.status(status).json(exception.getResponse());
  }
}