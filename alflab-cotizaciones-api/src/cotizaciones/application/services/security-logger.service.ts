import { Injectable, Logger } from '@nestjs/common';
import * as fs from 'node:fs';
import * as path from 'node:path';

@Injectable()
export class SecurityLoggerService {
  // Usamos el logger nativo de NestJS para pintar en consola
  private readonly logger = new Logger('SeguridadAlfLab');
  
  // Definimos la ruta donde se guardará la evidencia (en la raíz del proyecto)
  private readonly logFilePath = path.join(process.cwd(), 'evidencia-ataques.log');

  registrarAtaque(tipoAtaque: string, ip: string, endpoint: string, detalle: string) {
    const fecha = new Date().toISOString();
    
    // Formato exacto que pidió el maestro (control de dónde y cómo)
    const mensajeEvidencia = `[FECHA]: ${fecha} | [IP]: ${ip} | [TIPO DE ATAQUE]: ${tipoAtaque} | [ENDPOINT]: ${endpoint} | [DETALLE]: ${detalle}\n`;

    // 1. Lo imprimimos en la consola de la terminal (saldrá en rojo para que el maestro lo vea en vivo)
    this.logger.error(`¡Intento de ataque detectado! Tipo: ${tipoAtaque} en ${endpoint}`);

    // 2. Lo guardamos en el archivo de texto para dejar evidencia permanente
    try {
      fs.appendFileSync(this.logFilePath, mensajeEvidencia);
    } catch (error) {
      this.logger.error('No se pudo escribir el archivo de evidencia', error);
    }
  }
}