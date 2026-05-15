-- scripts/replica-init.sql
STOP REPLICA;

-- Configuramos el puente usando Auto-Posicionamiento (GTID)
CHANGE REPLICATION SOURCE TO 
SOURCE_HOST='db-master', 
SOURCE_USER='replicador', 
SOURCE_PASSWORD='alflab_replica', 
SOURCE_AUTO_POSITION = 1, 
GET_SOURCE_PUBLIC_KEY=1;

START REPLICA;