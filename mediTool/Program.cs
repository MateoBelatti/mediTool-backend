using Microsoft.EntityFrameworkCore;
using mediTool.Middlewares;
using mediTool.Mappers;
using Biblioteca.Repository;
using Repository.Pacientes;
using Repository.Profesionales;
using Repository.TurnosFijos;
using Repository.Turnos;
using Service.Pacientes;
using Service.Profesionales;
using Service.TurnosFijos;
using Service.Turnos;
using Repository.Informes;
using Repository.Reuniones;
using Repository.Notificaciones;
using Service.Informes;
using Service.Reuniones;
using Service.Notificaciones;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Service.Auth;
using mediTool.Security;
using mediTool.Backgrounds;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Inyección de dependencias de Repositorios
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IProfesionalRepository, ProfesionalRepository>();
builder.Services.AddScoped<ITurnoFijoRepository, TurnoFijoRepository>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<IInformeRepository, InformeRepository>();
builder.Services.AddScoped<IReunionRepository, ReunionRepository>();
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();

// Inyección de dependencias de Servicios
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IProfesionalService, ProfesionalService>();
builder.Services.AddScoped<ITurnoFijoService, TurnoFijoService>();
builder.Services.AddScoped<IGenerarInstanciasTurnoService, GenerarInstanciasTurnoService>();
builder.Services.AddScoped<ITurnoService, TurnoService>();
builder.Services.AddScoped<IInformeService, InformeService>();
builder.Services.AddScoped<IReunionService, ReunionService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Background services
builder.Services.AddHostedService<NotificacionAsistenciasPendientesService>();

// JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
    if (key.Length < 32)
    {
        throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Profesional", policy => policy.RequireRole("Profesional"));
    options.AddPolicy("AdminOrProfesional", policy => policy.RequireRole("Admin", "Profesional"));
});

builder.Services.AddCors(options =>
{
    var origenesPermitidos = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173", "http://[::1]:5173" };

    options.AddPolicy("FrontendCors",
        policy =>
        {
            policy.WithOrigins(origenesPermitidos)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Ensure database is created and migrations applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    mediTool.Seeders.AdminSeeder.SeedAdmin(dbContext, configuration);
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// El CORS va antes de la redirección HTTPS para que el preflight OPTIONS
// responda 204 con los headers CORS en vez de un 307 que el navegador no
// puede resolver de forma consistente.
app.UseCors("FrontendCors");

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.Run();

