namespace AlfLab.Api.Domain.Entities
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CodigoLaboratorio { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadStock { get; set; }
        
    }
}