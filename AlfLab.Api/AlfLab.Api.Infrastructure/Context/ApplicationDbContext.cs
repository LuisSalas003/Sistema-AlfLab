using AlfLab.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlfLab.Api.Infrastructure.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Tus tablas principales
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }

        // Tu nueva tabla de auditoría para los ataques
        public DbSet<RegistroAuditoria> RegistrosAuditoria { get; set; }
    }
}