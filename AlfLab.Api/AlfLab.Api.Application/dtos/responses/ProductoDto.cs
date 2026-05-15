namespace AlfLab.Api.Application.dtos
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadStock { get; set; }
    }
}