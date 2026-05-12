import { Injectable, NestMiddleware } from '@nestjs/common';
import { Request, Response, NextFunction } from 'express';
import { ReplicaMonitorService } from './replica-monitor.service';

@Injectable()
export class ReplicaCheckMiddleware implements NestMiddleware {
  // Inyectamos el servicio para poder leer el semáforo
  constructor(private readonly monitor: ReplicaMonitorService) {}

  use(req: Request, res: Response, next: NextFunction) {
    // Lectura en nanosegundos directamente de la RAM
    if (!this.monitor.isReplicaActive) {
      return res.status(200).json({
        success: false,
        message: 'El sistema está realizando rutinas de auto-recuperación y sincronización en segundo plano. Por favor, intente su operación en unos segundos.'
      });
    }
    
    // Si está verde, pasa instantáneamente
    next();
  }
}