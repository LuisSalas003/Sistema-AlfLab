using AlfLab.Api.Application.Interfaces;
using AlfLab.Api.Domain.Entities;
using AlfLab.Api.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlfLab.Api.Infrastructure.Repositories
{
    // Implementamos la misma interfaz, pero ahora con lógica real
    public class ProductoRepository : IProductoRepository
    {
        private readonly ApplicationDbContext _context;

        // Inyectamos el puente (DbContext) a través del constructor
        public ProductoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Producto>> ObtenerTodosAsync()
        {
            // Va a MySQL, trae todos los productos y los convierte en una lista
            return await _context.Productos.ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            // Busca un producto por su Id en MySQL
            return await _context.Productos.FindAsync(id);
        }

        public async Task AgregarAsync(Producto producto)
        {
            await _context.Productos.AddAsync(producto);
            await _context.SaveChangesAsync(); // Esto ejecuta el INSERT en MySQL
        }
    }
}