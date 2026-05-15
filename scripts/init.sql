-- scripts/init.sql

-- Para el ecosistema .NET
CREATE DATABASE IF NOT EXISTS alflab_principal;

-- Para el ecosistema NestJS (Master)
CREATE DATABASE IF NOT EXISTS alflab_cotizaciones;

-- Opcional: Crear el usuario de replicación aquí mismo para ahorrar tiempo
CREATE USER IF NOT EXISTS 'replicador'@'%' IDENTIFIED BY 'alflab_replica';
GRANT REPLICATION SLAVE ON *.* TO 'replicador'@'%';
FLUSH PRIVILEGES;