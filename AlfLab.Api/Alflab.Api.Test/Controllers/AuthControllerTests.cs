using Alflab.Api.Test.Config;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Alflab.Api.Test.Controllers
{
    [Collection("PruebasSecuenciales")] // <--- ESTA ETIQUETA
    public class AuthControllerTests : IClassFixture<AlfLabTestFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(AlfLabTestFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ObtenerToken_ConLoginValido_DeberiaRetornarOk()
        {
            // --- ARRANGE ---
            // Usamos un Guid para evitar colisiones si se corre al mismo tiempo que las pruebas de Productos
            var correoPrueba = $"auth_{Guid.NewGuid()}@alflab.com";
            var passwordPrueba = "Password123!";
            
            var usuarioFalso = new { nombreCompleto = "Tester Auth", correo = correoPrueba, password = passwordPrueba };
            await _client.PostAsJsonAsync("/api/Auth/registrar", usuarioFalso);

            var loginData = new { correo = correoPrueba, password = passwordPrueba };

            // --- ACT ---
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginData);

            // --- ASSERT ---
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var contenido = await response.Content.ReadFromJsonAsync<LoginResponseTest>();
            Assert.NotNull(contenido);
            Assert.NotNull(contenido.Token);
        }
        [Fact]
        public async Task RateLimiter_AtaqueDeFuerzaBruta_DeberiaBloquearPeticiones()
        {
            // --- ARRANGE ---
            // ¡EL SECRETO! Creamos un servidor nuevo y aislado solo para este ataque.
            // Así no gasta los boletos (permits) del servidor principal de las otras pruebas.
            using var fabricaAislada = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>();
            var clienteHacker = fabricaAislada.CreateClient();

            var loginData = new { correo = "hacker@alflab.com", password = "123" };
            HttpResponseMessage? ultimaRespuesta = null;

            // --- ACT ---
            for (int i = 0; i < 6; i++)
            {
                ultimaRespuesta = await clienteHacker.PostAsJsonAsync("/api/Auth/login", loginData);
            }

            // --- ASSERT ---
            Assert.NotNull(ultimaRespuesta);
            Assert.Equal(System.Net.HttpStatusCode.TooManyRequests, ultimaRespuesta.StatusCode);
        }
        // Eliminamos RefreshToken para que SonarQube no marque advertencias de variables sin uso
        // Al hacerla pública, SonarQube entiende que un agente externo (el Deserializador JSON) lxa usará
        public class LoginResponseTest
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}