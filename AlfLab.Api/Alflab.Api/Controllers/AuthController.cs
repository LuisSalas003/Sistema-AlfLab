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
        private readonly IAuditoriaRepository _auditoriaRepository;

        public AuthController(
            IUsuarioRepository usuarioRepository, 
            IConfiguration config, 
            ILogger<AuthController> logger,
            IAuditoriaRepository auditoriaRepository) 
        {
            _usuarioRepository = usuarioRepository;
            _config = config;
            _logger = logger;
            _auditoriaRepository = auditoriaRepository;
        }

        [HttpPost("registrar")]
        [Authorize (Roles = "Admin")] 
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioRequestDto request)
        {
            // =========================================================
            // DEFENSA XSS CON PERSISTENCIA EN BASE DE DATOS
            // =========================================================
            if (request.NombreCompleto.Contains("<") || request.NombreCompleto.Contains(">") || request.NombreCompleto.Contains("script"))
            {
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

            // EF Core se encarga del cifrado de forma invisible al buscar
            var usuarioExistente = await _usuarioRepository.ObtenerPorCorreoAsync(request.Correo);
            if (usuarioExistente != null)
            {
                return BadRequest(new { mensaje = "El correo ya está registrado en el sistema." });
            }

            // Enviamos los datos en texto plano; el DbContext los encriptará al hacer el SaveChanges
            var nuevoUsuario = new Usuario
            {
                NombreCompleto = request.NombreCompleto,
                Correo = request.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Rol = request.Rol
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

            // 1. SI LA CUENTA YA ESTÁ BLOQUEADA (Retornamos 429 en lugar de 401)
            if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.Now)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new { mensaje = "Cuenta bloqueada temporalmente por múltiples intentos fallidos." });
            }

            bool passwordValido = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
            
            if (!passwordValido)
            {
                usuario.IntentosFallidos += 1;
                
                var ataque = new RegistroAuditoria
                {
                    TipoAtaque = "Fuerza Bruta",
                    CorreoInvolucrado = request.Correo,
                    Detalles = $"Intento fallido #{usuario.IntentosFallidos} de 5."
                };
                await _auditoriaRepository.GuardarAtaqueAsync(ataque);

                // 2. SI EN ESTE INTENTO LLEGÓ AL LÍMITE (Bloqueamos y retornamos 429)
                if (usuario.IntentosFallidos >= 5)
                {
                    usuario.BloqueadoHasta = DateTime.Now.AddMinutes(1);
                    _logger.LogCritical("🚨 SEGURIDAD: Usuario {Correo} bloqueado.", request.Correo);
                    
                    await _usuarioRepository.ActualizarAsync(usuario); 
                    
                    return StatusCode(StatusCodes.Status429TooManyRequests, new { mensaje = "Límite de intentos excedido. Cuenta bloqueada temporalmente." });
                }

                // 3. SI FALLÓ PERO AÚN TIENE INTENTOS (Retornamos 401 normal)
                await _usuarioRepository.ActualizarAsync(usuario); 
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });
            }

            // --- LOGIN EXITOSO ---
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

            // Nota: Al generar el Token, 'usuario.Correo' ya vendrá desencriptado automáticamente por EF Core
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Correo),
                new Claim("id", usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol)
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