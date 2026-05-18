import { 
  Controller, 
  Post, 
  UseInterceptors, 
  UploadedFile, 
  BadRequestException 
} from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { diskStorage } from 'multer';
import { extname } from 'node:path'; 
import { v4 as uuidv4 } from 'uuid';
// 👇 1. IMPORTAMOS LA "CLASE/INTERFAZ" OFICIAL DE NESTJS
import { MulterOptions } from '@nestjs/platform-express/multer/interfaces/multer-options.interface';

const TAMANO_MAXIMO_BYTES = 5 * 1024 * 1024; // 5 MB

// 👇 2. LE ASIGNAMOS EL TIPO ESTRICTO AL OBJETO
const configArchivos: MulterOptions = {
  storage: diskStorage({
    destination: './uploads', 
    filename: (req, file, cb) => {
      const nombreAleatorio = uuidv4();
      cb(null, `${nombreAleatorio}${extname(file.originalname)}`);
    }
  }),
  limits: { // NOSONAR
    fileSize: TAMANO_MAXIMO_BYTES 
  },
  fileFilter: (req, file, cb) => {
    const extensionesPermitidas = /\.(pdf|jpg|jpeg|png)$/i;
    
    if (!extensionesPermitidas.test(file.originalname)) {
      return cb(new BadRequestException('Solo se permiten archivos PDF, JPG, JPEG o PNG.'), false);
    }
    cb(null, true);
  }
};

@Controller('archivos')
export class ArchivosController {

  @Post('subir')
  @UseInterceptors(FileInterceptor('archivo', configArchivos)) // 👈 Pasamos el objeto tipado
  subirArchivo(@UploadedFile() archivo: Express.Multer.File) {
    if (!archivo) {
      throw new BadRequestException('No se proporcionó ningún archivo.');
    }

    return {
      mensaje: 'Archivo subido y validado con éxito.',
      nombreOriginal: archivo.originalname,
      nombreAlmacenado: archivo.filename,
      tamanoBytes: archivo.size
    };
  }
}