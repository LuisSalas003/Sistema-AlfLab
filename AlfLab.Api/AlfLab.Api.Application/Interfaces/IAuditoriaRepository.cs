using AlfLab.Api.Domain.Entities;
using System.Threading.Tasks;
// Prueba de pipeline independiente
namespace AlfLab.Api.Application.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task GuardarAtaqueAsync(RegistroAuditoria auditoria);
    }
}