import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { CotizacionesModule } from './cotizaciones/cotizaciones.module';
import { ScheduleModule } from '@nestjs/schedule';
import { ConfigModule } from '@nestjs/config';
import * as dotenv from 'dotenv';
import { join } from 'node:path';
import { AuthModule } from './auth/auth.module';
import { SeguridadModule } from './seguridad/seguridad.module';

// Obligamos a dotenv a buscar el archivo .env exactamente un nivel arriba de la carpeta 'src' (o 'dist')
dotenv.config({ path: join(__dirname, '../.env') }); 

@Module({
  imports: [
    ScheduleModule.forRoot(),
    ConfigModule.forRoot({ isGlobal: true }), // <-- Lo moví arriba para que cargue primero
    TypeOrmModule.forRoot({
      type: 'mysql',
      // EL SECRETO DE CQRS: Le enseñamos a TypeORM quién es quién
      replication: {
        master: {
          host: process.env.DB_HOST || '127.0.0.1',
          port: Number.parseInt(process.env.DB_PORT || '3309', 10), // 3309 es el Master
          username: process.env.DB_USER || 'root',
          password: process.env.DB_PASSWORD,
          database: process.env.DB_NAME,
        },
        slaves: [
          {
            host: process.env.DB_HOST || '127.0.0.1', 
            port: Number.parseInt(process.env.PORT_DB_REPLICA || '3307', 10), // 3307 es la Réplica
            username: process.env.DB_REPLICA_USER || 'root', // Leemos tu .env
            password: process.env.DB_REPLICA_PASSWORD || process.env.DB_PASSWORD,
            database: process.env.DB_NAME,
          }
        ]
      },
      entities: [__dirname + '/**/*.orm-entity{.ts,.js}'],
      autoLoadEntities: true,
      synchronize: true, 
    }),
    SeguridadModule,
    CotizacionesModule,
    AuthModule,
  ],
})
export class AppModule {}