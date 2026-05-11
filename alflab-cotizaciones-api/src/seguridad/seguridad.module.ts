import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { APP_FILTER } from '@nestjs/core';
// Importamos la entidad desde tu carpeta central
import { AuditoriaOrmEntity } from '../cotizaciones/infrastructure/database/entities/auditoria.orn-entity';
import { SecurityAuditFilter } from './security-audit.filter';

@Module({
  imports: [
    TypeOrmModule.forFeature([AuditoriaOrmEntity]), 
  ],
  providers: [
    {
      provide: APP_FILTER,
      useClass: SecurityAuditFilter,
    },
  ],
})
export class SeguridadModule {}