using System;

namespace AlfLab.Api.Domain.Entities
{
    public class RegistroAuditoria
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string TipoAtaque { get; set; } = string.Empty;
        public string CorreoInvolucrado { get; set; } = string.Empty;
        public string Detalles { get; set; } = string.Empty;
    }
}