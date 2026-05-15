# 🏢 Sistema AlfLab - Arquitectura Híbrida y CQRS

![NestJS](https://img.shields.io/badge/NestJS-E0234E?style=for-the-badge&logo=nestjs&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-005C84?style=for-the-badge&logo=mysql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

Plataforma de gestión empresarial construida bajo el patrón de segregación de responsabilidades (CQRS), utilizando microservicios y replicación de bases de datos en tiempo real.

## 🚀 Arquitectura del Sistema

El proyecto está dividido en dos ecosistemas principales que interactúan de forma segura:

1. **API Principal (.NET):** Encargada de la gestión central, autenticación de usuarios y exposición de servicios generales.
2. **API de Cotizaciones (NestJS):** Microservicio especializado con un Cron Job interno (Robot de Facturación) para procesamiento automático.
3. **Bases de Datos Replicadas:**
   - **Master (MySQL):** Recibe todas las transacciones de escritura (POST, PUT, DELETE).
   - **Réplica (MySQL):** Base de datos de solo lectura sincronizada en tiempo real, utilizada por el Cron de NestJS para lecturas pesadas sin afectar el rendimiento transaccional.

## 🛡️ Seguridad Implementada

- **Autenticación JWT:** Firmas criptográficas y validación estricta de tokens.
- **Anti-Suplantación (Fingerprinting):** Validación de sesión mediante rastreo de IP de conexión.
- **Rate Limiting:** Protección activa contra ataques de fuerza bruta y denegación de servicio (DDoS).
- **Persistencia de Datos:** Volúmenes de Docker configurados localmente para evitar pérdida de información ante reinicios críticos del contenedor.

---

## ⚙️ Guía de Instalación (Para Evaluación)

Sigue estos pasos para levantar el entorno completo de forma local utilizando Docker Compose.

### 1. Requisitos Previos
- Tener instalado [Docker](https://www.docker.com/products/docker-desktop/) y Docker Compose.
- Puerto `3001` (NestJS), `5000` (.NET) y `3306-3309` (MySQL) liberados.

### 2. Configuración del Entorno
Por seguridad, las credenciales reales no se encuentran en este repositorio. 
1. Crea un archivo llamado `.env` en la raíz del proyecto.
2. Copia el contenido del archivo `.env.example`.
3. Asigna las contraseñas reales y la llave secreta (JWT) que utilizarás para la evaluación.

### 3. Levantar los Servicios
Abre tu terminal en la raíz del proyecto y ejecuta el siguiente comando para construir las imágenes y levantar el ecosistema:

```bash
docker-compose up -d --build