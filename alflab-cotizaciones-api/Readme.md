# Sistema AlfLab - API de Cotizaciones

## 🚀 Ejecución rápida (Para evaluación)
Para levantar todo el ecosistema (Bases de datos + API NestJS + Cron):
1. Clonar el repositorio.
2. Ejecutar `docker-compose up -d`.

> **🛡️ Nota de Seguridad Arquitectónica:** 
> Por estándares de seguridad industrial, el archivo `.env` original fue agregado al `.gitignore` para no exponer credenciales reales en el repositorio. 
> Sin embargo, para cumplir con el requisito de ejecución "One-Click", el archivo `docker-compose.yml` ha sido configurado con **Fallback Variables** (`:-`). Si no detecta un `.env`, inyectará credenciales de desarrollo locales automáticamente para que el sistema funcione sin configuración adicional.