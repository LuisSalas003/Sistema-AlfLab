using AlfLab.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AlfLab.Api.Infrastructure.Security;

namespace AlfLab.Api.Infrastructure.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RegistroAuditoria> RegistrosAuditoria { get; set; } // 👈 Mantenemos tu tabla de auditoría

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // CONFIGURACIÓN DE ENCRIPTACIÓN AUTOMÁTICA (RESTAURADA)
            // =========================================================
            
            // Le decimos a EF Core que encripte y desencripte el Nombre automáticamente
            modelBuilder.Entity<Usuario>()
                .Property(u => u.NombreCompleto)
                .HasConversion(
                    v => EncryptionHelper.Encrypt(v),
                    v => EncryptionHelper.Decrypt(v)
                );

            // Hacemos lo mismo con el Correo
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Correo)
                .HasConversion(
                    v => EncryptionHelper.Encrypt(v),
                    v => EncryptionHelper.Decrypt(v)
                );

            // =========================================================
            // CONFIGURACIÓN DE TABLA DE AUDITORÍA
            // =========================================================
            modelBuilder.Entity<RegistroAuditoria>().ToTable("RegistrosAuditoria");
        }
    }
}