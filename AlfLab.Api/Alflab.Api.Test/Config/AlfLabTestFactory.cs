using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AlfLab.Api.Infrastructure.Contexts; // Tu ruta correcta

namespace Alflab.Api.Test.Config
{
    public class AlfLabTestFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Le gritamos a Program.cs: "¡Somos las pruebas, no prendas MySQL!"
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Como la silla está vacía, sentamos a la memoria RAM tranquilamente
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("AlfLabTestDb");
                });
            });
        }
    }
}