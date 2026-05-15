using AlfLab.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using AlfLab.Api.Domain.Entities;
using AlfLab.Api.Application.dtos.requests;

namespace AlfLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Esto hace que la URL sea: /api/productos
    public class ProductosController : ControllerBase
    {
        private readonly IProductoRepository _repository;

        // Inyectamos la interfaz, NO la clase concreta. ¡Puro SOLID!
        public ProductosController(IProductoRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerInventario()
        {
            var productos = await _repository.ObtenerTodosAsync();
            return Ok(productos);
        }
        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] CrearProductoRequestDto request)
        {
            // Transformamos el DTO que viene de internet a nuestra Entidad pura del Dominio
            var nuevoProducto = new Producto
            {
                Nombre = request.Nombre,
                CodigoLaboratorio = request.CodigoLaboratorio,
                Categoria = request.Categoria,
                CantidadStock = request.CantidadStock
            };

            await _repository.AgregarAsync(nuevoProducto);
            
            return Ok(new { mensaje = "Producto registrado con éxito en el sistema AlfLab" });
        }
    }
}