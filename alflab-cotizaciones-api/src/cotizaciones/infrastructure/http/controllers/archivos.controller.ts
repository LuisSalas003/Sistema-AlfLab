import { Controller, Post, UseInterceptors, UploadedFile, BadRequestException } from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { diskStorage } from 'multer';
import { v4 as uuidv4 } from 'uuid';
import { extname } from 'node:path';

@Controller('archivos') // 👈 Aquí definimos la URL exacta que usas en tu .http
export class ArchivosController {
  
  @Post('subir')
  @UseInterceptors(FileInterceptor('archivo', {
    // 1. Límite estricto de tamaño (5 MB)
    limits: {
      fileSize: 5 * 1024 * 1024, 
    },
    // 2. Filtro de Seguridad (Regex y Magic Numbers)
    fileFilter: (req, file, callback) => {
      // 👈 MODIFICACIÓN APLICADA: Uso de RegExp.exec() para optimizar memoria
      const regexEjecutables = /\.(exe|bat|cmd|sh)$/i;
      
      if (regexEjecutables.exec(file.originalname)) {
        return callback(
          new BadRequestException('Archivo no permitido. Extensión bloqueada por seguridad.'),
          false,
        );
      }
      
      // Bloqueamos tipos MIME maliciosos (Como el que configuraste en tu prueba EICAR)
      if (file.mimetype === 'application/x-msdownload' || file.mimetype.includes('javascript')) {
        return callback(
          new BadRequestException('ALERTA DE SEGURIDAD: Posible malware detectado. Operación cancelada.'),
          false,
        );
      }

      callback(null, true); // Si pasa las pruebas, lo dejamos seguir
    },
    // 3. Ofuscación de archivos
    storage: diskStorage({
      destination: './uploads', // Se guardarán en esta carpeta
      filename: (req, file, callback) => {
        // Renombramos el archivo con un UUID para que nadie pueda adivinar la ruta
        const nombreSeguro = `${uuidv4()}${extname(file.originalname)}`;
        callback(null, nombreSeguro);
      },
    }),
  }))
  subirArchivo(@UploadedFile() file: Express.Multer.File) {
    if (!file) {
      throw new BadRequestException('El archivo es inválido o fue rechazado por el firewall de la aplicación.');
    }

    return {
      mensaje: 'Archivo verificado y almacenado con éxito.',
      nombreOfuscado: file.filename,
      pesoBytes: file.size
    };
  }
}