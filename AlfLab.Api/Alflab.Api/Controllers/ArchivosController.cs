using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AlfLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArchivosController : ControllerBase
    {
        // Definimos el límite de tamaño (ej. 5 MB)
        private const long TamanoMaximoBytes = 5 * 1024 * 1024;
        
        // Lista blanca de extensiones seguras permitidas
        private readonly string[] ExtensionesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };

        [HttpPost("subir")]
        public async Task<IActionResult> SubirArchivo(IFormFile archivo)
        {
            // Validar si viene vacío
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new { mensaje = "No se ha seleccionado ningún archivo." });
            }

            // 1. FILTRO DE SEGURIDAD 1: Validar el tamaño máximo
            if (archivo.Length > TamanoMaximoBytes)
            {
                return BadRequest(new { mensaje = "El archivo excede el límite permitido de 5 MB." });
            }

            // 2. FILTRO DE SEGURIDAD 2: Validar extensión inyectada
            var extension = Path.GetExtension(archivo.FileName).ToLowerString();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                return BadRequest(new { mensaje = "Extensión no permitida. Solo se aceptan archivos PDF, JPG, JPEG y PNG." });
            }

            try
            {
                // Ruta interna del contenedor donde se guardarán los archivos
                var carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                
                // Si la carpeta no existe en el servidor, la creamos
                if (!Directory.Exists(carpetaDestino))
                {
                    Directory.CreateDirectory(carpetaDestino);
                }

                // 3. FILTRO DE SEGURIDAD 3: Renombrar el archivo con un GUID único
                // Esto evita que un usuario suba un archivo que sobrescriba el de otro usuario
                var nombreUnicoArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(carpetaDestino, nombreUnicoArchivo);

                // Guardar físicamente el archivo en el disco
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                return Ok(new 
                { 
                    mensaje = "Archivo subido y validado con éxito.",
                    nombreOriginal = archivo.FileName,
                    nombreAlmacenado = nombreUnicoArchivo,
                    tamano = $"{archivo.Length / 1024} KB"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno al guardar el archivo: {ex.Message}" });
            }
        }
    }
}

// Extensión de apoyo rápida para limpiar el texto de la extensión
public static class StringExtensions
{
    public static string ToLowerString(this string str) => str?.ToLower() ?? string.Empty;
}