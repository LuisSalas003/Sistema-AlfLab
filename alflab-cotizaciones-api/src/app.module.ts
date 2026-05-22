import { Module, MiddlewareConsumer, NestModule } from '@nestjs/common'; // 👈 Se agregaron MiddlewareConsumer y NestModule
import { TypeOrmModule } from '@nestjs/typeorm';
import { CotizacionesModule } from './cotizaciones/cotizaciones.module';
import { ScheduleModule } from '@nestjs/schedule';
import { ConfigModule } from '@nestjs/config';
import * as dotenv from 'dotenv';
import { join } from 'node:path';
import { AuthModule } from './auth/auth.module';
import { SeguridadModule } from './seguridad/seguridad.module';
import { ReplicaMonitorService } from './seguridad/replica-monitor.service'; 
import { ReplicaCheckMiddleware } from './seguridad/replica-check.middleware';
import { ArchivosController } from './cotizaciones/infrastructure/http/controllers/archivos.controller';

// Obligamos a dotenv a buscar el archivo .env exactamente un nivel arriba de la carpeta 'src' (o 'dist')
dotenv.config({ path: join(__dirname, '../../.env') });

@Module({
  imports: [
    ScheduleModule.forRoot(),
    ConfigModule.forRoot({ isGlobal: true }), 
   TypeOrmModule.forRoot({
      type: 'mysql',
      // EL SECRETO DE CQRS: Le enseñamos a TypeORM quién es quién
      replication: {
        master: {
          host: process.env.DB_HOST || 'alflab_db_master', // Directo a tu Master
          port: 3306, 
          username: 'root',
          password: process.env.DB_PASSWORD,
          database: process.env.DB_NAME,
        },
        slaves: [
          {
            host: process.env.DB_REPLICA_HOST || 'alflab_db_replica', // Directo a tu Réplica
            port: 3306, 
            username: 'root', 
            password: process.env.DB_PASSWORD,
            database: process.env.DB_NAME,
          }
        ]
      },
      // 👈 EL ARREGLO: Le decimos que busque CUALQUIER archivo que diga 'entity'
      entities: ['dist/**/*.js'],
      autoLoadEntities: true,
      synchronize: true, 
    }),
    SeguridadModule,
    CotizacionesModule,
    AuthModule,
  ],
  controllers: [ArchivosController], // 👈 2. Agrégalo aquí
  providers: [ReplicaMonitorService], // 👈 Registramos el Vigía para que el Cron Job empiece a trabajar
})
export class AppModule implements NestModule { // 👈 Implementamos NestModule para poder usar Middlewares
  
  // 👈 Esta función intercepta TODAS las peticiones y las pasa por nuestro Guardia
  configure(consumer: MiddlewareConsumer) {
    consumer
      .apply(ReplicaCheckMiddleware)
      .forRoutes('*'); 
  }
}