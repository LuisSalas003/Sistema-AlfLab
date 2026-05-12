import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import { ValidationPipe } from '@nestjs/common';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Set global API prefix
  app.setGlobalPrefix('api');

  // Enable global validation
  app.useGlobalPipes(new ValidationPipe());
  
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