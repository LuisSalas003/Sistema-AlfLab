using AlfLab.Api.Application.Interfaces;
using AlfLab.Api.Application.dtos.requests;
using AlfLab.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace AlfLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _config;

        // Inyectamos el repositorio y la configuración (para leer la llave secreta del appsettings.json)
        public AuthController(IUsuarioRepository usuarioRepository, IConfiguration config)
        {
            _usuarioRepository = usuarioRepository;
            _config = config;
        }

[HttpPost("registrar")]
[Authorize] // 👈 EL CANDADO MÁGICO: Exige que traigan un Token JWT válido
public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioRequestDto request)
{
    // 1. Verificar si el usuario ya existe
    var usuarioExistente = await _usuarioRepository.ObtenerPorCorreoAsync(request.Correo);
    if (usuarioExistente != null)
    {
        return BadRequest(new { mensaje = "El correo ya está registrado en el sistema." });
    }

    // 2. Si no existe, procedemos con el registro protegido
    var nuevoUsuario = new Usuario
    {
        NombreCompleto = request.NombreCompleto,
        Correo = request.Correo,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        // Lo ideal es que el DTO traiga el rol, pero si no, ponle uno de bajo nivel por defecto
        Rol = "Ventas" 
    };

    await _usuarioRepository.AgregarAsync(nuevoUsuario);
    return Ok(new { mensaje = "Usuario registrado exitosamente por el administrador." });
}

        [HttpPost("login")]
        [EnableRateLimiting("ReglaLoginEstricto")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(request.Correo);
            if (usuario == null)
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });

            bool passwordValido = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
            if (!passwordValido)
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });
            // 1. Fabricamos la Llave 1 (El JWT de 30 segundos)
            var token = GenerarJwtToken(usuario);

            // 2. Fabricamos la Llave 2 (El Refresh Token de 7 días)
            var refreshToken = GenerarRefreshToken();

            // 3. Guardamos la Llave 2 en el usuario y actualizamos la base de datos
            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _usuarioRepository.ActualizarAsync(usuario);

            // 4. Entregamos ambas llaves en la respuesta
            return Ok(new 
            { 
                token = token,
                refreshToken = refreshToken,
                mensaje = "Login exitoso. Guarda bien tus llaves."
            });
        }

        // Método privado de apoyo para fabricar la llave
        private string GenerarJwtToken(Usuario usuario)
        {
            // Leemos la llave secreta que pusiste en appsettings.json
          var keyInfo = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "EstaEsUnaLlaveDeRespaldoPorSiFallaElEnv123!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyInfo));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Guardamos datos del usuario DENTRO del token (Claims)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Correo),
                new Claim("id", usuario.Id.ToString()),
                new Claim("rol", usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), 
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerarRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        
        
    }
}