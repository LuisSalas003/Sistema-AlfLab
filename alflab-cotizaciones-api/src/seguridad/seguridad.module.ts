import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { APP_FILTER } from '@nestjs/core';
// Importamos la entidad desde tu carpeta central
import { AuditoriaOrmEntity } from '../cotizaciones/infrastructure/database/entities/auditoria.orn-entity';
import { SecurityAuditFilter } from './security-audit.filter';
import { ReplicaMonitorService } from './replica-monitor.service';

@Module({
  imports: [
    TypeOrmModule.forFeature([AuditoriaOrmEntity]), 
  ],
  providers: [
    {
      provide: APP_FILTER,
      useClass: SecurityAuditFilter,
    }, // 👈 Aquí se cierra la configuración del filtro
    ReplicaMonitorService, // 👈 Ahora sí, es un proveedor independiente en la lista
  ],
  exports: [ReplicaMonitorService], // 👈 Esto permite que otros módulos lo usen
})
export class SeguridadModule {}