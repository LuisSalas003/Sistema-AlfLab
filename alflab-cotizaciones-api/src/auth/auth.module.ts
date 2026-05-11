import { Module } from '@nestjs/common';
import { ConfigModule, ConfigService } from '@nestjs/config'; 
import { JwtModule } from '@nestjs/jwt';
import { PassportModule } from '@nestjs/passport';
import { TypeOrmModule } from '@nestjs/typeorm'; // <-- 1. Importa TypeORM

import { AuthService } from '../cotizaciones/application/services/auth.service'; 
import { AuthController } from '../cotizaciones/infrastructure/http/controllers/auth.controller';
import { JwtStrategy } from '../auth/infrastructure/strategies/jwt.strategy'; 

// <-- 2. Importa la Entidad de Usuario (asegúrate de que la ruta sea la correcta usando el autocompletado de VS Code)
import { UsuarioOrmEntity } from '../cotizaciones/infrastructure/database/entities/usuario.orm-entity'; 

@Module({
  imports: [
    ConfigModule, // Solucionó el error anterior
    TypeOrmModule.forFeature([UsuarioOrmEntity]), // <-- 3. ¡LA LLAVE DE LA BASE DE DATOS!
    PassportModule.register({ defaultStrategy: 'jwt' }), 
    JwtModule.registerAsync({
      imports: [ConfigModule], 
      inject: [ConfigService], 
      useFactory: (configService: ConfigService) => ({
        secret: configService.get<string>('JWT_SECRET'),
        signOptions: { expiresIn: '2h' },
      }),
    }),
  ],
  controllers: [AuthController],
  providers: [
    AuthService, 
    JwtStrategy 
  ],
})
export class AuthModule {}