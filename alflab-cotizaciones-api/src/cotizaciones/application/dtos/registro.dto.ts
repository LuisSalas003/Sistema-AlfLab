import { IsEmail, IsNotEmpty, MinLength } from 'class-validator';

export class RegistroDto {
  @IsEmail({}, { message: 'Debe ser un correo electrónico válido.' })
  email!: string;

  @IsNotEmpty()
  @MinLength(6, { message: 'La contraseña debe tener al menos 6 caracteres.' })
  password!: string;
}