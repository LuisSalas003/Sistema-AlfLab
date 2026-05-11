import { BadRequestException, Injectable, UnauthorizedException } from '@nestjs/common';
import { JwtService } from '@nestjs/jwt';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import * as bcrypt from 'bcrypt'; 
import { LoginDto } from '../dtos/login.dto'; 
import { RegistroDto } from '../dtos/registro.dto';
import { UsuarioOrmEntity } from '../../infrastructure/database/entities/usuario.orm-entity';

@Injectable()
export class AuthService {
  constructor(
    private readonly jwtService: JwtService,
    @InjectRepository(UsuarioOrmEntity) 
    private readonly usuarioRepo: Repository<UsuarioOrmEntity>,
  ) {}

  async login(datos: LoginDto, ip: string) {
    const usuarioEncontrado = await this.usuarioRepo.findOne({
      where: { email: datos.usuario } 
    });

    if (!usuarioEncontrado) {
      throw new UnauthorizedException('Credenciales incorrectas');
    }

    if (usuarioEncontrado.bloqueadoHasta && usuarioEncontrado.bloqueadoHasta > new Date()) {
      const tiempoRestante = Math.ceil((usuarioEncontrado.bloqueadoHasta.getTime() - Date.now()) / 60000);
      throw new UnauthorizedException(`Cuenta bloqueada temporalmente por seguridad. Intente de nuevo en ${tiempoRestante} minutos.`);
    }

    const passwordCoincide = await bcrypt.compare(datos.password, usuarioEncontrado.password);

    if (!passwordCoincide) {
      usuarioEncontrado.intentosFallidos = (usuarioEncontrado.intentosFallidos || 0) + 1;

      if (usuarioEncontrado.intentosFallidos >= 3) {
        const fechaDesbloqueo = new Date();
        fechaDesbloqueo.setMinutes(fechaDesbloqueo.getMinutes() + 15);
        usuarioEncontrado.bloqueadoHasta = fechaDesbloqueo;
      }

      await this.usuarioRepo.save(usuarioEncontrado);
      throw new UnauthorizedException('Credenciales incorrectas');
    }

    if (usuarioEncontrado.intentosFallidos > 0 || usuarioEncontrado.bloqueadoHasta) {
      usuarioEncontrado.intentosFallidos = 0;
      usuarioEncontrado.bloqueadoHasta = null;
      await this.usuarioRepo.save(usuarioEncontrado);
    }

    const payload = { 
      nombreUsuario: usuarioEncontrado.email, 
      rol: 'ADMINISTRADOR',
      sub: usuarioEncontrado.id,
      fingerprint: ip
    };

    return {
      mensaje: 'Autenticación exitosa',
      token_acceso: this.jwtService.sign(payload),
    };
  }

  async registrar(dto: RegistroDto) {
    const usuarioExistente = await this.usuarioRepo.findOne({ 
      where: { email: dto.email } 
    });
    
    if (usuarioExistente) {
      throw new BadRequestException('Ese correo ya está registrado en AlfLab.');
    }

    const salt = await bcrypt.genSalt(10);
    const passwordEncriptada = await bcrypt.hash(dto.password, salt);

    const nuevoUsuario = this.usuarioRepo.create({
      email: dto.email,
      password: passwordEncriptada,
      intentosFallidos: 0,
      bloqueadoHasta: null
    });

    await this.usuarioRepo.save(nuevoUsuario);

    return {
      mensaje: 'Usuario maestro creado exitosamente. Ya puedes iniciar sesión.',
      email: nuevoUsuario.email
    };
  }
}