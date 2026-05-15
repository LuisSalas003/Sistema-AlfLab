using AlfLab.Api.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlfLab.Api.Application.Interfaces
{
    public interface IProductoRepository
    {
        // Solo definimos QUÉ queremos hacer, no CÓMO se hace.
        Task<IEnumerable<Producto>> ObtenerTodosAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        // Agrega esta línea debajo de las otras dos
        Task AgregarAsync(Producto producto);
    }
}