import { IsNotEmpty, IsNumber, IsString, Matches } from 'class-validator';
import { ApiProperty } from '@nestjs/swagger'; 

export class CrearCotizacionDto {
  @ApiProperty() 
  @IsString({ message: 'El clienteId debe ser un texto' })
  @IsNotEmpty({ message: 'El clienteId es obligatorio' })
  // 👇 CORRECCIÓN 1: El guion va al final de los corchetes, sin diagonal
  @Matches(/^[a-zA-Z0-9-]+$/, {
    message: 'ALERTA DE SEGURIDAD: El clienteId contiene caracteres no permitidos.'
  })
  clienteId!: string;

  @ApiProperty()
  @IsNumber({}, { message: 'El total debe ser un número' })
  @IsNotEmpty({ message: 'El total es obligatorio' })
  total!: number;
  
  @ApiProperty()
  @IsString({ message: 'El concepto debe ser un texto' })
  @IsNotEmpty({ message: 'El concepto es obligatorio' })
  // 👇 CORRECCIÓN 2: El punto va sin diagonal, y el guion al mero final
  @Matches(/^[a-zA-Z0-9 áéíóúÁÉÍÓÚñÑ.,-]+$/, {
    message: 'ALERTA DE SEGURIDAD: Intento de inyección de comandos bloqueado.'
  })
  concepto!: string; 
}