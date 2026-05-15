// Ruta: src/auth/infrastructure/guards/jwt-auth.guard.ts
import { Injectable, ExecutionContext, UnauthorizedException } from '@nestjs/common';
import { AuthGuard } from '@nestjs/passport';

@Injectable()
export class JwtAuthGuard extends AuthGuard('jwt') {
  canActivate(context: ExecutionContext) {
    // Aquí puedes agregar logs de auditoría en el futuro si lo deseas
    return super.canActivate(context);
  }

  handleRequest(err: any, user: any, info: any) {
    // Lanzamos un error limpio y estandarizado si falla el token o no existe
    if (err || !user) {
      throw err || new UnauthorizedException('Acceso denegado. Token faltante, inválido o expirado.');
    }
    return user;
  }
}