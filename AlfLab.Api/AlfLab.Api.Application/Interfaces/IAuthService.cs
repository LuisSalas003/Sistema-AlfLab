namespace AlfLab.Api.Application.Interfaces
{
    public interface IAuthService
    {
        Task<(string token, string refreshToken, string mensaje)> LoginAsync(string correo, string password, string ip);
    }
}