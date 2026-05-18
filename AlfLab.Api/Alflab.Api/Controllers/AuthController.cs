using AlfLab.Api.Application.Interfaces;
using AlfLab.Api.Application.dtos.requests;
using AlfLab.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AlfLab.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuditoriaRepository _auditoriaRepository; // 👈 Agregamos el repositorio

        public AuthController(
            IUsuarioRepository usuarioRepository, 
            IConfiguration config, 
            ILogger<AuthController> logger,
            IAuditoriaRepository auditoriaRepository) // 👈 Lo inyectamos
        {
            _usuarioRepository = usuarioRepository;
            _config = config;
            _logger = logger;
            _auditoriaRepository = auditoriaRepository;
        }

        [HttpPost("registrar")]
        [Authorize] 
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioRequestDto request)
        {
            // =========================================================
            // DEFENSA XSS CON PERSISTENCIA EN BASE DE DATOS
            // =========================================================
            if (request.NombreCompleto.Contains("<") || request.NombreCompleto.Contains(">") || request.NombreCompleto.Contains("script"))
            {
                // Guardamos el ataque en la base de datos de manera inmutable
                var ataque = new RegistroAuditoria
                {
                    TipoAtaque = "XSS",
                    CorreoInvolucrado = request.Correo,
                    Detalles = $"Intento de inyección en NombreCompleto. Payload: {request.NombreCompleto}"
                };
                await _auditoriaRepository.GuardarAtaqueAsync(ataque);

                _logger.LogWarning("🛡️ ALERTA DE SEGURIDAD: XSS guardado en base de datos para: {Correo}", request.Correo);
                
                return BadRequest(new { 
                    mensaje = "ALERTA DE SEGURIDAD: Inyección XSS detectada y bloqueada." 
                });
            }

            var usuarioExistente = await _usuarioRepository.ObtenerPorCorreoAsync(request.Correo);
            if (usuarioExistente != null)
            {
                return BadRequest(new { mensaje = "El correo ya está registrado en el sistema." });
            }

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = request.NombreCompleto,
                Correo = request.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Rol = "Ventas" 
            };

            await _usuarioRepository.AgregarAsync(nuevoUsuario);
            return Ok(new { mensaje = "Usuario registrado exitosamente." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(request.Correo);
            if (usuario == null)
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });

            if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.Now)
            {
                return Unauthorized(new { mensaje = "Cuenta bloqueada temporalmente." });
            }

            bool passwordValido = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
            
            if (!passwordValido)
            {
                usuario.IntentosFallidos += 1;
                
                // Guardamos el intento de Fuerza Bruta en la tabla de auditoría
                var ataque = new RegistroAuditoria
                {
                    TipoAtaque = "Fuerza Bruta",
                    CorreoInvolucrado = request.Correo,
                    Detalles = $"Intento fallido #{usuario.IntentosFallidos} de 5."
                };
                await _auditoriaRepository.GuardarAtaqueAsync(ataque);

                if (usuario.IntentosFallidos >= 5)
                {
                    usuario.BloqueadoHasta = DateTime.Now.AddMinutes(1);
                    _logger.LogCritical("🚨 SEGURIDAD: Usuario {Correo} bloqueado.", request.Correo);
                }

                await _usuarioRepository.ActualizarAsync(usuario); 
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });
            }

            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;

            var token = GenerarJwtToken(usuario);
            var refreshToken = GenerarRefreshToken();

            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _usuarioRepository.ActualizarAsync(usuario);

            return Ok(new { token = token, refreshToken = refreshToken });
        }

        private string GenerarJwtToken(Usuario usuario)
        {
            var keyInfo = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "EstaEsUnaLlaveDeRespaldoPorSiFallaElEnv123!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyInfo));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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