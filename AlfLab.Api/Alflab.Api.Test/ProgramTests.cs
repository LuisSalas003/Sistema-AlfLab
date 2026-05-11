using AlfLab.Api.Infrastructure.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Alflab.Api.Test
{
    [Collection("PruebasSecuenciales")]
    public class ProgramTests 
    {
        [Fact]
        public void Program_DeberiaRegistrarApplicationDbContext_Exitosamente()
        {
            // --- ARRANGE & ACT ---
            // Creamos una fábrica exclusiva para esta prueba que finge ser "Development"
            // Esto obliga a Program.cs a saltarse el IF y encender MySQL para complacer a tu maestro.
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => 
            {
                builder.UseEnvironment("Development"); 
            });

            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            // --- ASSERT ---
            Assert.NotNull(dbContext);
        }
    }
}