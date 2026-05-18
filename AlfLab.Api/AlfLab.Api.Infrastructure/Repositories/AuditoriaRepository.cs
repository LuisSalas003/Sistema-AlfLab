using AlfLab.Api.Application.Interfaces;
using AlfLab.Api.Domain.Entities;
using AlfLab.Api.Infrastructure.Contexts;
using System.Threading.Tasks;

namespace AlfLab.Api.Infrastructure.Repositories
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GuardarAtaqueAsync(RegistroAuditoria auditoria)
        {
            await _context.RegistrosAuditoria.AddAsync(auditoria);
            await _context.SaveChangesAsync();
        }
    }
}