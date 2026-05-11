import { Entity, Column, PrimaryGeneratedColumn } from 'typeorm';

@Entity('usuarios')
export class UsuarioOrmEntity {

  // 1. Contador de errores
  @Column({ type: 'int', default: 0 })
  intentosFallidos!: number;

  // 2. El reloj de arena del castigo
  @Column({ type: 'timestamp', nullable: true })
  bloqueadoHasta!: Date | null;

  @PrimaryGeneratedColumn('uuid')
  id!: string; // Generaremos este ID desde código o base de datos

  @Column({ type: 'varchar', unique: true })
  email!: string;

  @Column({ type: 'varchar' })
  password!: string;
}