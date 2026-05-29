using AlfLab.Api.Domain.Entities;
using System.Threading.Tasks;
// Prueba de pipeline
namespace AlfLab.Api.Application.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task GuardarAtaqueAsync(RegistroAuditoria auditoria);
    }
}