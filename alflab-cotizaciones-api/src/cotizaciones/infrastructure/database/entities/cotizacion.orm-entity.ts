import { Entity, Column, PrimaryColumn, CreateDateColumn, UpdateDateColumn } from 'typeorm';

@Entity('cotizaciones')
export class CotizacionOrmEntity {
  @PrimaryColumn('uuid') id!: string;
  @Column({ type: 'varchar', unique: true }) folio!: string;
  @Column({ type: 'varchar' }) clienteId!: string;
  @Column('decimal', { precision: 10, scale: 2 }) total!: number;
  @Column({ type: 'varchar', length: 20 }) estado!: string;
  @CreateDateColumn() fechaCreacion!: Date;
  @UpdateDateColumn() fechaActualizacion!: Date;
}