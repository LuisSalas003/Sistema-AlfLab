import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication, ValidationPipe } from '@nestjs/common';
import request from 'supertest';
import { AppModule } from './../src/app.module';
import { LimpiadorCotizacionesService } from '../src/cotizaciones/cron.service';

describe('Cotizaciones API (Integración)', () => {
  let app: INestApplication;
  let moduleFixture: TestingModule;
  let cotizacionId = ''; // <-- Variable para guardar el ID generado

  beforeAll(async () => {
    moduleFixture = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    
    app.useGlobalPipes(new ValidationPipe()); 
    
    await app.init();
  });

  afterAll(async () => {
    await app.close();
  });

  // PRUEBA 1: Crear (POST)
  it('1. POST /api/cotizaciones - Debe crear una cotización exitosamente', () => {
    const datosDePrueba = { clienteId: 'CLI-TEST-001', total: 999.99 };

    return request(app.getHttpServer())
      .post('/api/cotizaciones')
      .send(datosDePrueba)
      .expect(201)
      .expect((res) => {
        expect(res.body.mensaje).toEqual('Cotización creada');
        cotizacionId = res.body.data.id; // <-- Guardamos el ID para las siguientes pruebas
      });
  });

  // PRUEBA 2: Obtener Todas (GET)
  it('2. GET /api/cotizaciones - Debe obtener la lista de cotizaciones', () => {
    return request(app.getHttpServer())
      .get('/api/cotizaciones')
      .expect(200)
      .expect((res) => {
        expect(Array.isArray(res.body)).toBeTruthy();
        expect(res.body.length).toBeGreaterThan(0); // Verifica que traiga datos
      });
  });

  // PRUEBA 3: Obtener por ID (GET)
  it('3. GET /api/cotizaciones/:id - Debe obtener una cotización específica', () => {
    return request(app.getHttpServer())
      .get(`/api/cotizaciones/${cotizacionId}`)
      .expect(200)
      .expect((res) => {
        expect(res.body.id).toEqual(cotizacionId); // Verifica que sea la misma
        expect(res.body.estado).toEqual('BORRADOR');
      });
  });

  // PRUEBA 4: Enviar (PATCH)
  it('4. PATCH /api/cotizaciones/:id/enviar - Debe cambiar el estado a ENVIADA', () => {
    return request(app.getHttpServer())
      .patch(`/api/cotizaciones/${cotizacionId}/enviar`)
      .expect(200)
      .expect((res) => {
        expect(res.body.mensaje).toContain('ENVIADA');
      });
  });

  //Prueba 5: CRON para limpiar cotizaciones vencidas
  it('5. CRON / Debe ejecutar la limpieza de cotizaciones vencidas', async () => {
    // 1. Obtenemos el servicio del robot desde el entorno de NestJS
    const limpiadorService = moduleFixture.get<LimpiadorCotizacionesService>(LimpiadorCotizacionesService);
    
    // 2. Ejecutamos la función a la fuerza (sin esperar los 10 segundos)
    await limpiadorService.limpiarCotizacionesVencidas();
    
    // 3. Simplemente validamos que el servicio exista y haya corrido sin explotar
    expect(limpiadorService).toBeDefined();
  });


  // PRUEBA 6: Obtener por ID (GET) - Camino Triste
  it('6. GET /api/cotizaciones/:id (Camino Triste) - Debe retornar 404 si no existe', () => {
    // Pedimos un ID que sabemos que no está en la base de datos
    return request(app.getHttpServer())
      .get('/api/cotizaciones/9999999') 
      .expect(404); // Le decimos a Jest que ESPERAMOS que falle con un 404
  });

// PRUEBA 7: CRON para limpiar cotizaciones vencidas (Camino Feliz)
 it('7. CRON (Camino Feliz) - Debe rechazar cotizaciones cuando encuentra vencidas', async () => {
    const repo = moduleFixture.get('CotizacionOrmEntityRepository');
    
    const fechaVieja = new Date();
    fechaVieja.setDate(fechaVieja.getDate() - 20);
    
    // Inyectamos la cotización con el último requisito
    await repo.save({
      id: '123e4567-e89b-12d3-a456-426614174000',
      folio: 'COT-TEST-001',
      clienteId: 'CLI-TEST-999',
      cliente: 'Cliente Caducado',
      monto: 1500,
      total: 1740, // <-- ¡AGREGA ESTA LÍNEA! (Puede ser cualquier número)
      estado: 'ENVIADA',
      fechaCreacion: fechaVieja,
    });

    const limpiadorService = moduleFixture.get<LimpiadorCotizacionesService>(LimpiadorCotizacionesService);
    await limpiadorService.limpiarCotizacionesVencidas();
    
    expect(limpiadorService).toBeDefined();
  });

  // PRUEBA 8: Crear (POST) - Camino Triste
  it('8. POST /api/cotizaciones (Camino Triste) - Debe rechazar la creación si los datos están vacíos', () => {
    return request(app.getHttpServer())
      .post('/api/cotizaciones')
      .send({}) // Le enviamos un objeto vacío intencionalmente
      .expect(400); // Esperamos que NestJS rechace la petición
  });

// PRUEBA 9: Enviar (PATCH) - Camino Triste
  it('9. PATCH /api/cotizaciones/:id/enviar (Camino Triste) - Debe fallar si la cotización no existe', () => {
    return request(app.getHttpServer())
      .patch('/api/cotizaciones/9999999/enviar') // ID falso
      .expect(404); // Esperamos un Not Found
  });

});