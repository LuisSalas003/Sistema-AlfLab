import { Entity, Column, PrimaryGeneratedColumn, CreateDateColumn } from 'typeorm';

@Entity('registro_seguridad')
export class AuditoriaOrmEntity {
  @PrimaryGeneratedColumn('uuid')
  id!: string;

  @Column()
  ip!: string; 

  @Column()
  ruta!: string; 

  @Column()
  metodo! : string; 

  @Column({ type: 'text', nullable: true })
  payload!: string; 

  @Column({ type: 'text' })
  motivoRechazo!: string; 

  @CreateDateColumn()
  fechaAtaque! : Date; 
}