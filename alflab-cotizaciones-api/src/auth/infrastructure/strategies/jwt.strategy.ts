import { Injectable, UnauthorizedException } from '@nestjs/common';
import { PassportStrategy } from '@nestjs/passport';
import { ExtractJwt, Strategy } from 'passport-jwt';
import { ConfigService } from '@nestjs/config';
import { Request } from 'express'; 

@Injectable()
export class JwtStrategy extends PassportStrategy(Strategy) {
  constructor(private readonly configService: ConfigService) {
    const jwtSecret = configService.get<string>('JWT_SECRET');
    if (!jwtSecret) {
      throw new Error('JWT_SECRET is not defined');
    }

    super({
      jwtFromRequest: ExtractJwt.fromAuthHeaderAsBearerToken(),
      ignoreExpiration: false,
      secretOrKey: jwtSecret,
      passReqToCallback: true, 
    });
  }

  async validate(req: Request, payload: any) {
    const ipActual = req.ip || req.socket.remoteAddress || 'IP_DESCONOCIDA';

    if (payload.fingerprint && payload.fingerprint !== ipActual) {
      throw new UnauthorizedException('Suplantación detectada: Sesión inválida para esta conexión.');
    }

    return { userId: payload.sub, username: payload.nombreUsuario, rol: payload.rol };
  }
}