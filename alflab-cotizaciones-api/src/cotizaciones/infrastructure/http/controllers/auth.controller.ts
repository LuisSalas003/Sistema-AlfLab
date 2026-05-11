import { Controller, Post, Body, HttpCode, HttpStatus, Ip } from '@nestjs/common';
import { AuthService } from '../../../application/services/auth.service';
import { LoginDto } from '../../../application/dtos/login.dto';
import { RegistroDto } from '../../../application/dtos/registro.dto';

@Controller('auth')
export class AuthController {
  constructor(private readonly authService: AuthService) {}

  @Post('login')
  @HttpCode(HttpStatus.OK)
  async iniciarSesion(@Body() body: LoginDto, @Ip() ip: string) {
    return this.authService.login(body, ip || 'IP_DESCONOCIDA');
  }

  @Post('registro')
  async registrarUsuario(@Body() body: RegistroDto) {
    return this.authService.registrar(body); 
  }
}