import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { JwtModule } from '@nestjs/jwt'; 
import { ConfigModule, ConfigService } from '@nestjs/config';
import { CotizacionOrmEntity } from './infrastructure/database/entities/cotizacion.orm-entity';
import { CotizacionesController } from './infrastructure/http/controllers/cotizaciones.controller';
import { CrearCotizacionService } from './application/services/crear-cotizacion.service';
import { ObtenerCotizacionesService } from './application/services/obtener-cotizaciones.service';
import { ObtenerCotizacionPorIdService } from './application/services/obtener-cotizacion-por-id.service';
import { EnviarCotizacionService } from './application/services/enviar-cotizacion.service';
import { CotizacionTypeOrmRepository } from './infrastructure/database/repositories/cotizacion-typeorm.repository';
import { COTIZACION_REPOSITORY } from './domain/interfaces/cotizacion.repository';
import { SecurityLoggerService } from './application/services/security-logger.service';
import { CotizacionesCronService } from './application/services/cotizaciones-cron.service'; 
import { SeguridadModule } from '../seguridad/seguridad.module';

@Module({
  imports: [
    ConfigModule,
    TypeOrmModule.forFeature([CotizacionOrmEntity]), 
    JwtModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: async (configService: ConfigService) => ({
        secret: configService.get<string>('JWT_SECRET') || 'SecretoDeRespaldoDeEmergencia',
        signOptions: { expiresIn: '1h' },
      }),
    }),
    SeguridadModule, 
  ],
  controllers: [
    CotizacionesController
  ],
  providers: [
    SecurityLoggerService,
    CrearCotizacionService,
    ObtenerCotizacionesService,
    ObtenerCotizacionPorIdService,
    EnviarCotizacionService,
    CotizacionesCronService, 
    { provide: COTIZACION_REPOSITORY, useClass: CotizacionTypeOrmRepository },
  ],
})
export class CotizacionesModule {}