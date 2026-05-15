using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO; // Añadido para manejar las rutas de carpetas

// Agregamos las rutas reales hacia tus otras capas de Arquitectura
using AlfLab.Api.Infrastructure.Contexts;
using AlfLab.Api.Infrastructure.Repositories;
using AlfLab.Api.Application.Interfaces;
using AlfLab.Api.Application.Services;
using AlfLab.Api.Security; // Para el robot de mantenimiento y el monitor de réplica
using AlfLab.Api.Middlewares;  
using Microsoft.AspNetCore.RateLimiting;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// 👇 1. PUENTE INVISIBLE AL .ENV DE NESTJS (Seguridad de Arquitecto)
// Viajamos dos carpetas hacia atrás y entramos a la de Nest para leer tu archivo maestro
var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../alflab-cotizaciones-api/.env"));
if (File.Exists(envPath))
{
    Env.Load(envPath);
}
builder.Configuration.AddEnvironmentVariables();

// 👇 2. BASE DE DATOS (Inteligencia de Entornos)
// Extraemos tu contraseña real del .env (si no lo encuentra, usa root por defecto)
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "root";

// Si Docker nos manda la conexión (producción), la usamos. 
// Si estamos en Local (dotnet run), apuntamos al puerto 3308 dinámicamente usando tu contraseña real.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? $"Server=127.0.0.1;Port=3308;Database=alflab_principal;Uid=root;Pwd={dbPassword};";

if (builder.Environment.EnvironmentName != "Testing")
{
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Error Crítico: No se encontró la cadena de conexión 'DefaultConnection' en el archivo de configuración.");
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30)))
    );
}

// 3. INYECCIÓN DE DEPENDENCIAS
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddHostedService<RobotMantenimientoService>(); // Registramos el robot de mantenimiento como servicio de fondo
builder.Services.AddSingleton<EstadoSistema>(); // Estado compartido para el monitor de réplica
builder.Services.AddHostedService<MonitorReplicaService>(); // Registramos el monitor de réplica como servicio de fondo

// 4. AUTENTICACIÓN JWT (El Candado)
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "EstaEsUnaLlaveDeRespaldoPorSiFallaElEnv123!";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };

        // VALIDACIÓN ANTI-CLONACIÓN POR IP
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var ipActual = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP_DESCONOCIDA";
                var ipEnToken = context.Principal?.FindFirst("fingerprint")?.Value;

                if (!string.IsNullOrEmpty(ipEnToken) && ipEnToken != ipActual)
                {
                    context.Fail("Suplantación detectada: Sesión inválida para esta conexión.");
                }

                return Task.CompletedTask;
            }
        };
    });

// 5. RATE LIMITER (El Escudo siempre encendido)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("ReglaLoginEstricto", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 6. SWAGGER
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sistema AlfLab API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorización JWT. Escribe 'Bearer' [espacio] y luego tu token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// 👇 1. ACTIVAR EL ESCUDO GLOBAL PRIMERO QUE NADA
app.UseMiddleware<AlfLab.Api.Middlewares.ExceptionMiddleware>();

// 👇 2. ACTIVAR EL ESCUDO DE LA RÉPLICA
app.UseMiddleware<AlfLab.Api.Middlewares.ReplicaCheckMiddleware>();

// MIDDLEWARES
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Activamos el escudo para todos
app.UseRateLimiter();

// El orden es vital: Primero quién eres, luego qué puedes hacer
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 👇 CREACIÓN AUTOMÁTICA AL ARRANCAR 👇
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Esto crea las tablas automáticamente en la BD de Docker si no existen
        await context.Database.EnsureCreatedAsync(); 
        Console.WriteLine("✅ Base de datos migrada y lista para usarse.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al migrar la base de datos: {ex.Message}");
    }
}
// 👆 FIN DEL BLOQUE 👆

await app.RunAsync();

// 7. EXPOSICIÓN PARA PRUEBAS (Para que el Factory pueda verlo)
public partial class Program 
{ 
    protected Program() { } 
}