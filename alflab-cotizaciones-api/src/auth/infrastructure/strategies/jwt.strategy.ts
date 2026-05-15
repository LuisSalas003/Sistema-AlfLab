import { Injectable, UnauthorizedException } from '@nestjs/common';
import { PassportStrategy } from '@nestjs/passport';
import { ExtractJwt, Strategy } from 'passport-jwt';
import { ConfigService } from '@nestjs/config';
import { Request } from 'express';

@Injectable()
export class JwtStrategy extends PassportStrategy(Strategy) {
  constructor(private readonly configService: ConfigService) {
    super({
      jwtFromRequest: ExtractJwt.fromAuthHeaderAsBearerToken(),
      ignoreExpiration: false,
      // 👇 EL TRUCO: Le damos un valor de respaldo para que TypeScript sepa que NUNCA será undefined
      secretOrKey: configService.get<string>('JWT_SECRET') || 'Firma_Local_De_Prueba_2026',
      passReqToCallback: true, 
    });
  }

  async validate(req: Request, payload: any) {
    // Extraemos la IP real de la petición actual
    const ipActual = req.ip || req.socket.remoteAddress;
    const ipEnToken = payload.fingerprint;

    // Validación estricta anti-suplantación de sesión
    if (ipEnToken && ipEnToken !== ipActual) {
      throw new UnauthorizedException('Suplantación detectada: Sesión inválida para esta conexión.');
    }

    return { userId: payload.sub, email: payload.email, roles: payload.roles };
  }
}