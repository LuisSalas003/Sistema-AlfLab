namespace AlfLab.Api.Application.dtos.requests
{
    public class LoginRequestDto
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}