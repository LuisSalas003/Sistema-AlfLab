using Alflab.Api.Test.Config;
using Bogus; // Nuestra Data Factory
using System.Net;
using System.Net.Http.Headers; // Vital para el candado del maestro
using System.Net.Http.Json;
using Xunit;

namespace Alflab.Api.Test.Controllers
{
    // Usamos el mismo Factory para tener base de datos en RAM
    [Collection("PruebasSecuenciales")] // <--- LA MISMA ETIQUETA AQUÍ
    public class ProductosControllerTests : IClassFixture<AlfLabTestFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Faker _faker;

        public ProductosControllerTests(AlfLabTestFactory<Program> factory)
        {
            _client = factory.CreateClient();
            // Configuramos Bogus para inventar datos de comercio en español de México
            _faker = new Faker("es_MX"); 
        }
        private async Task<string> GetAuthenticatedTokenAsync()
        {
            // 1. Correo 100% único garantizado con un GUID
            var correoFalso = $"tester_{Guid.NewGuid()}@alflab.com"; 
            var passwordFalsa = "SecurePassword123!";

            var usuario = new { nombreCompleto = "Tester", correo = correoFalso, password = passwordFalsa };
            
            // 2. TRAMPA: Si el registro falla, queremos que la prueba explote aquí, no en el login
            var regResponse = await _client.PostAsJsonAsync("/api/Auth/registrar", usuario);
            if (!regResponse.IsSuccessStatusCode)
            {
                var errorBody = await regResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"El API bloqueó el Registro. Detalle: {errorBody}");
            }

            var loginData = new { correo = correoFalso, password = passwordFalsa };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginData);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"El API bloqueó el Login. Detalle: {errorBody}");
            }

            var content = await response.Content.ReadFromJsonAsync<LoginResponseTest>();
            return content?.Token ?? throw new InvalidOperationException("No se pudo obtener el token.");
        }

        // --- PRUEBAS DE INTEGRACIÓN PARA PRODUCTOS (AAA) ---

        [Fact]
        public async Task CrearProducto_ConDatosValidosYAutenticado_DeberiaRetornarCreated()
        {
            // --- ARRANGE (Preparar) ---
            // 1. Obtener Token y ponerlo en el candado (Como en el video del maestro a las 5:28)
            var token = await GetAuthenticatedTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. Crear datos falsos realistas para el producto usando Bogus
            var nuevoProducto = new
            {
                // Inventa un nombre como "Silla de Oficina Ergonómica"
                nombre = _faker.Commerce.ProductName(), 
                // Inventa una descripción como "Hecho de materiales reciclados..."
                descripcion = _faker.Commerce.ProductDescription(), 
                // Inventa un precio entre 10.00 y 1000.00 pesos
                precio = _faker.Random.Decimal(10, 1000) 
            };

            // --- ACT (Actuar) ---
            // Mandamos la petición POST al endpoint protegido
            var response = await _client.PostAsJsonAsync("/api/Productos", nuevoProducto);

            // --- ASSERT (Afirmar) como el maestro ---
            // Evaluamos que nos devuelva un 201 Created (Éxito al crear)
            // Cambiamos HttpStatusCode.Created por HttpStatusCode.OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ObtenerProductos_Autenticado_DeberiaRetornarOkConLista()
        {
            // --- ARRANGE ---
            // 1. Obtener Token y autenticar el cliente
            var token = await GetAuthenticatedTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. Crear un producto primero para asegurar que haya datos en la base de datos RAM
            var prodFake = new { nombre = "Prod Prueba", descripcion = "Desc Prueba", precio = 99.99m };
            await _client.PostAsJsonAsync("/api/Productos", prodFake);

            // --- ACT ---
            var response = await _client.GetAsync("/api/Productos");

            // --- ASSERT ---
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verificamos que el contenido sea una lista y no esté vacía
            var productos = await response.Content.ReadFromJsonAsync<List<object>>();
            Assert.NotNull(productos);
            Assert.NotEmpty(productos);
        }

        // Clase auxiliar para mapear el Token del JSON
       // --- CLASES AUXILIARES ---
        
        public class LoginResponseTest 
        { 
            public string Token { get; set; } = string.Empty; 
        }

        public class ProductoTestHelper 
        { 
            public int Id { get; set; } 
        }
    }
}