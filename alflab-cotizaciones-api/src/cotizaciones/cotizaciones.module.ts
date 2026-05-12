import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { JwtModule } from '@nestjs/jwt'; 
import { ConfigModule, ConfigService } from '@nestjs/config';
import { CotizacionOrmEntity } from './infrastructure/database/entities/cotizacion.orm-entity';
import { UsuarioOrmEntity } from './infrastructure/database/entities/usuario.orm-entity'; 
import { CotizacionesController } from './infrastructure/http/controllers/cotizaciones.controller';
import { CrearCotizacionService } from './application/services/crear-cotizacion.service';
import { ObtenerCotizacionesService } from './application/services/obtener-cotizaciones.service';
import { ObtenerCotizacionPorIdService } from './application/services/obtener-cotizacion-por-id.service';
import { EnviarCotizacionService } from './application/services/enviar-cotizacion.service';
import { CotizacionTypeOrmRepository } from './infrastructure/database/repositories/cotizacion-typeorm.repository';
import { COTIZACION_REPOSITORY } from './domain/interfaces/cotizacion.repository';
import { SecurityLoggerService } from './application/services/security-logger.service';
import { AuthController } from './infrastructure/http/controllers/auth.controller'; 
import { AuthService } from './application/services/auth.service'; 
import { CotizacionesCronService } from './application/services/cotizaciones-cron.service'; // <-- Tu único servicio de fondo
import { SeguridadModule } from '../seguridad/seguridad.module';

@Module({
  imports: [
    ConfigModule,
    TypeOrmModule.forFeature([CotizacionOrmEntity, UsuarioOrmEntity]), 
    JwtModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: async (configService: ConfigService) => ({
        secret: configService.get<string>('JWT_SECRET') || 'SecretoDeRespaldoDeEmergencia',
        signOptions: { expiresIn: '1h' },
      }),
    }),
    SeguridadModule, // <-- Importamos el módulo de seguridad
  ],
  controllers: [
    CotizacionesController,
    AuthController
  ],
  providers: [
    SecurityLoggerService,
    CrearCotizacionService,
    ObtenerCotizacionesService,
    ObtenerCotizacionPorIdService,
    EnviarCotizacionService,
    CotizacionesCronService, // <-- Centralizado
    AuthService,
    { provide: COTIZACION_REPOSITORY, useClass: CotizacionTypeOrmRepository },
  ],
})
export class CotizacionesModule {}