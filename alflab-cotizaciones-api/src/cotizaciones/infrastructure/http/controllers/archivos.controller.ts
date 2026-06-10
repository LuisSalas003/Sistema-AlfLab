import { Controller, Post, UseInterceptors, UploadedFile, BadRequestException } from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { diskStorage } from 'multer';
import { v4 as uuidv4 } from 'uuid';
import { extname } from 'node:path';
import * as fs from 'node:fs'; //IMPORTANTE: Importamos 'fs' para leer/borrar del disco

@Controller('api/archivos') 
export class ArchivosController {
  
  @Post('subir')
  @UseInterceptors(FileInterceptor('archivo', {
    // 1. Límite estricto de tamaño (5 MB)
    limits: {
      fileSize: 5 * 1024 * 1024, 
    },
    // 2. Filtro de Seguridad (Regex y Magic Numbers)
    fileFilter: (req, file, callback) => {
      const regexEjecutables = /\.(exe|bat|cmd|sh)$/i;
      
      if (regexEjecutables.exec(file.originalname)) {
        return callback(
          new BadRequestException('Archivo no permitido. Extensión bloqueada por seguridad.'),
          false,
        );
      }
      
      if (file.mimetype === 'application/x-msdownload' || file.mimetype.includes('javascript')) {
        return callback(
          new BadRequestException('ALERTA DE SEGURIDAD: Posible malware detectado. Operación cancelada.'),
          false,
        );
      }

      callback(null, true);
    },
    // 3. Ofuscación de archivos
    storage: diskStorage({
      destination: './uploads', 
      filename: (req, file, callback) => {
        const nombreSeguro = `${uuidv4()}${extname(file.originalname)}`;
        callback(null, nombreSeguro);
      },
    }),
  }))
  subirArchivo(@UploadedFile() file: Express.Multer.File) {
    if (!file) {
      throw new BadRequestException('El archivo es inválido o fue rechazado por el firewall de la aplicación.');
    }

    // =========================================================
    // PARCHE DE SEGURIDAD: INSPECCIÓN PROFUNDA (EICAR)
    // =========================================================
    // Leemos el contenido real del archivo que se acaba de guardar
    const contenido = fs.readFileSync(file.path, 'utf8');
    
    // Si encontramos la firma del virus...
    if (contenido.includes('EICAR-STANDARD-ANTIVIRUS-TEST-FILE')) {
      fs.unlinkSync(file.path); // Lo destruimos inmediatamente del disco duro
      throw new BadRequestException('ALERTA DE SEGURIDAD CRÍTICA: Se detectó malware oculto en el documento. Archivo eliminado.');
    }
    // =========================================================

    return {
      mensaje: 'Archivo verificado y almacenado con éxito.',
      nombreOfuscado: file.filename,
      pesoBytes: file.size
    };
  }
}