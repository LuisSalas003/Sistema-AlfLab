using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection; 
using System.IO;
using System.Threading.Tasks; // Necesario para el Task.CompletedTask del JWT
using AlfLab.Api.Infrastructure.Contexts;
using AlfLab.Api.Infrastructure.Repositories;
using AlfLab.Api.Application.Interfaces;
using AlfLab.Api.Application.Services;
using AlfLab.Api.Security;
using AlfLab.Api.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using DotNetEnv;

//Comentario para compilar el workflow de GitHub Actions Api .Net

// --- PROGRAMA PRINCIPAL DE LA API DE ALFLAB ---
var builder = WebApplication.CreateBuilder(args);

// --- 1. CARGA DE VARIABLES Y ENTORNOS ---
var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../alflab-cotizaciones-api/.env"));
if (File.Exists(envPath)) Env.Load(envPath);
builder.Configuration.AddEnvironmentVariables();

// --- 2. PERSISTENCIA DE SEGURIDAD (Evita cierres de sesión al reiniciar Docker) ---
var keysDirectory = new DirectoryInfo("/app/keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDirectory)
    .SetApplicationName("AlfLab_System");

// --- 3. BASE DE DATOS (Configuración de MySql con fix de Clean Architecture) ---
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "root";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? $"Server=db-alflab-dotnet;Port=3306;Database=alflab_principal;Uid=root;Pwd={dbPassword};";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30)),
        // ESTA ES LA LÍNEA QUE REPARA EL ERROR DE LAS MIGRACIONES
        b => b.MigrationsAssembly("Alflab.Api")) 
);

// --- 4. INYECCIÓN DE DEPENDENCIAS ---
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddHostedService<RobotMantenimientoService>(); 
builder.Services.AddSingleton<EstadoSistema>(); 
builder.Services.AddHostedService<MonitorReplicaService>(); 
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

// --- 5. AUTENTICACIÓN JWT (Con Validación Anti-Clonación) ---
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

// --- 6. RATE LIMITER ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("ReglaLoginEstricto", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- 7. SWAGGER ---
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sistema AlfLab API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[]{} }
    });
});

var app = builder.Build();

// --- MIDDLEWARES ---
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<ReplicaCheckMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// --- 🚀 INICIALIZACIÓN PROFESIONAL DE LA BD ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // 1. Ejecuta las migraciones existentes del proyecto
        await context.Database.MigrateAsync(); 

        // Si la tabla no existe en MySQL, este script la crea al vuelo inmediatamente
        var sqlAuditoria = @"
            CREATE TABLE IF NOT EXISTS RegistrosAuditoria (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Fecha DATETIME(6) NOT NULL,
                TipoAtaque VARCHAR(50) NOT NULL,
                CorreoInvolucrado VARCHAR(255) NOT NULL,
                Detalles TEXT NOT NULL
            );";
        
        await context.Database.ExecuteSqlRawAsync(sqlAuditoria);
        
        Console.WriteLine("✅ Base de datos sincronizada (Migraciones y Tabla de Auditoría aplicadas con éxito).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error Crítico en Base de Datos: {ex.Message}");
    }
}

await app.RunAsync();

public partial class Program { protected Program() { } }