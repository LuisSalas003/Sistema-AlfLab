using AlfLab.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AlfLab.Api.Infrastructure.Security;

namespace AlfLab.Api.Infrastructure.Contexts
{
    // Heredamos de DbContext, que es la clase de Microsoft que hace la magia de BD
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Esta propiedad representa tu tabla en MySQL. 
        // El nombre que le pongas aquí ("Productos") será el nombre de la tabla.
        public DbSet<Producto> Productos { get; set; }
        
        // Agregamos la nueva tabla para el sistema de seguridad
        public DbSet<Usuario> Usuarios { get; set; }

        // --- AQUÍ AGREGAMOS LA CONFIGURACIÓN DE ENCRIPTACIÓN ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}