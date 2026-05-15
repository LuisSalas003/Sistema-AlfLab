import { IsNotEmpty, IsNumber, IsString } from 'class-validator';
import { ApiProperty } from '@nestjs/swagger'; // (Si estás usando Swagger)

export class CrearCotizacionDto {
  @ApiProperty() // <- Opcional, solo si usas Swagger
  @IsString({ message: 'El clienteId debe ser un texto' })
  @IsNotEmpty({ message: 'El clienteId es obligatorio' })
  clienteId!: string;

  @ApiProperty()
  @IsNumber({}, { message: 'El total debe ser un número' })
  @IsNotEmpty({ message: 'El total es obligatorio' })
  total!: number;
  
  // Agrega aquí otros campos que tu base de datos exija para crear
}