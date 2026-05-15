namespace AlfLab.Api.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        
        // Datos básicos
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        
        // Seguridad y Roles (Las que pedía el AuthController)
        public string PasswordHash { get; set; } = string.Empty; 
        public string Rol { get; set; } = "Usuario"; 
        
        // Tokens
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Nuestro Escudo Anti-Fuerza Bruta
        public int IntentosFallidos { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
    }
}