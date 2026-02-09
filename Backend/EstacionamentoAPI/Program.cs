using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Events;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Interfaces;
using EstacionamentoAPI.Repositories;
using EstacionamentoAPI.Services;
using EstacionamentoAPI.Middleware;
using EstacionamentoAPI.HealthChecks;

// ========================================
// 📝 CONFIGURAR SERILOG (LOGGING ESTRUTURADO)
// ========================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "EstacionamentoAPI")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: "logs/estacionamento-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30
    )
    .CreateLogger();

try
{
    Log.Information("🚀 Iniciando EstacionamentoAPI...");

    var builder = WebApplication.CreateBuilder(args);

    // Usar Serilog para logging
    builder.Host.UseSerilog();

// Configurar para rodar na porta 5000
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger com autenticação JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "EstacionamentoAPI", 
        Version = "v1",
        Description = "API para controle de estacionamento com autenticação JWT"
    });

    // Adicionar definição de segurança JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}"
    });

    // Adicionar requisito de segurança global
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar DbContext com SQLite
builder.Services.AddDbContext<EstacionamentoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar Repositories
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<ISessaoRepository, SessaoRepository>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();
builder.Services.AddScoped<IConfiguracaoRepository, ConfiguracaoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Registrar Services
builder.Services.AddScoped<ITokenService, TokenService>();

// Configurar JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ChaveSecretaSuperSegura@Estacionamento2026!";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "EstacionamentoAPI",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "EstacionamentoApp",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    });

    builder.Services.AddAuthorization();

    // ========================================
    // 🏥 CONFIGURAR HEALTHCHECKS
    // ========================================
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<EstacionamentoContext>("database")
        .AddCheck<DatabaseHealthCheck>("database_custom")
        .AddCheck<ConfigurationHealthCheck>("configuration");

    // Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

    var app = builder.Build();

    // ========================================
    // 🔍 USAR CORRELATION ID MIDDLEWARE
    // ========================================
    app.UseCorrelationId();

    // Criar banco de dados e aplicar migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EstacionamentoContext>();
    context.Database.EnsureCreated();

    // Criar usuário admin padrão se não existir
    if (!context.Usuarios.Any())
    {
        var adminUser = new EstacionamentoAPI.Models.Usuario
        {
            Nome = "Administrador",
            Email = "admin@estacionamento.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Perfil = EstacionamentoAPI.Models.Role.Admin,
            Ativo = true,
            DataCriacao = DateTime.Now
        };
            context.Usuarios.Add(adminUser);
            context.SaveChanges();
            
            Log.Information("✅ Usuário admin criado: {Email}", adminUser.Email);
        }
        else
        {
            Log.Information("ℹ️ Usuário admin já existe");
        }

        Log.Information("✅ Banco de dados inicializado com sucesso");
    }

    // ========================================
    // 🏥 CONFIGURAR ENDPOINTS DE HEALTHCHECK
    // ========================================
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                duration = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    data = e.Value.Data,
                    exception = e.Value.Exception?.Message
                })
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                })
            );
        }
    });

    // Healthcheck simples (apenas status)
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");

    Log.Information("✅ Healthcheck endpoints configurados: /health, /health/ready, /health/live");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

// Remover HTTPS redirect para desenvolvimento
// app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("🚀 EstacionamentoAPI iniciada com sucesso na porta 5000!");
    Log.Information("📊 Swagger UI: http://localhost:5000/swagger");
    Log.Information("🏥 Healthcheck: http://localhost:5000/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Aplicação falhou ao iniciar");
    throw;
}
finally
{
    Log.Information("🛑 Encerrando EstacionamentoAPI...");
    Log.CloseAndFlush();
}

// Tornar Program acessível para testes de integração
public partial class Program { }
