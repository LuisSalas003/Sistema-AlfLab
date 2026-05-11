using System.ComponentModel.DataAnnotations;

namespace AlfLab.Api.Application.dtos.requests // Tu namespace
{
    public class RegistroUsuarioRequestDto
    {
        [Required(ErrorMessage = "El nombre es estrictamente obligatorio.")]
        public required string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El correo es estrictamente obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo es inválido.")]
        public required string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres por seguridad.")]
        public required string Password { get; set; }
    }
}