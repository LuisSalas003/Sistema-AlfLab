import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import { ValidationPipe } from '@nestjs/common';

//Comentario para compilar el workflow de GitHub Actions Api NestJS

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Set global API prefix
  app.setGlobalPrefix('api');

  //Enable global validation (AHORA EN MODO ESTRICTO)
  app.useGlobalPipes(new ValidationPipe({
    whitelist: true, // Limpia silenciosamente cualquier campo que no esté definido en tu DTO
    forbidNonWhitelisted: true, // Si mandan un campo extra o basura, bloquea toda la petición con un error 400
  }));
  
  // 3. TERCERO construimos Swagger (¡Ahora con el candadito de vuelta!)
  const config = new DocumentBuilder()
    .setTitle('API de Cotizaciones - AlfLab')
    .setDescription('Microservicio para la gestión de cotizaciones')
    .setVersion('1.2')
    .addBearerAuth() // <-- ¡No olvides esto para el token!
    .build();
    
  const document = SwaggerModule.createDocument(app, config);
  
  // 4. CUARTO montamos la ruta. 
  // Al poner 'swagger' aquí, respetará la raíz que establecimos en el paso 1.
  SwaggerModule.setup('swagger', app, document); 

  await app.listen(3001, '0.0.0.0');
}
bootstrap();