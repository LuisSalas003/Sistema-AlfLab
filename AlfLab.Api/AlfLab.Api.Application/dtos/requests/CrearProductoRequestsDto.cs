namespace AlfLab.Api.Application.dtos.requests
{
    public class CrearProductoRequestDto
    {
        // Nota que no hay "Id" aquí. Solo lo que el usuario captura en el formulario de la página web.
        public string Nombre { get; set; } = string.Empty;
        public string CodigoLaboratorio { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadStock { get; set; }
    }
}