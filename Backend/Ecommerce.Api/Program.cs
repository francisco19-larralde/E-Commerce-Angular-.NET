using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using System.Text;
using Ecommerce.Api.Configuration;
using Ecommerce.Api.Health;
using Ecommerce.Api.Data;
using Ecommerce.Api.Services;
using Ecommerce.Api.Services.Interfaces;
using Ecommerce.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
if (builder.Environment.IsProduction())
{
    builder.Logging.AddJsonConsole();
}
else
{
    builder.Logging.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    });
}


builder.Services.AddControllers();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá el token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                []
        });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta ConnectionStrings:DefaultConnection. Copiá appsettings.Example.json como appsettings.Development.json o configurá la variable ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var maximoIntentosFallidos = builder.Configuration.GetValue<int?>(
    "Security:Lockout:MaxFailedAccessAttempts") ?? 5;
var minutosBloqueo = builder.Configuration.GetValue<int?>(
    "Security:Lockout:DefaultLockoutMinutes") ?? 15;
if (maximoIntentosFallidos <= 0 || minutosBloqueo <= 0)
{
    throw new InvalidOperationException("La configuración de bloqueo debe usar valores mayores a cero.");
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.SignIn.RequireConfirmedEmail = false;

    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = maximoIntentosFallidos;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(minutosBloqueo);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("Falta la sección de configuración Jwt.");
var erroresJwt = new List<ValidationResult>();
if (!Validator.TryValidateObject(jwtOptions, new ValidationContext(jwtOptions), erroresJwt, true))
{
    throw new InvalidOperationException(
        $"La configuración JWT no es válida: {string.Join(" | ", erroresJwt.Select(e => e.ErrorMessage))}");
}
builder.Services.AddSingleton(jwtOptions);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

var limiteAutenticacion = builder.Configuration.GetValue<int?>(
    "Security:RateLimit:Authentication:PermitLimit") ?? 5;
var ventanaAutenticacionMinutos = builder.Configuration.GetValue<int?>(
    "Security:RateLimit:Authentication:WindowMinutes") ?? 1;
if (limiteAutenticacion <= 0 || ventanaAutenticacionMinutos <= 0)
{
    throw new InvalidOperationException("La configuración de rate limiting debe usar valores mayores a cero.");
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("autenticacion", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "ip-desconocida",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limiteAutenticacion,
                Window = TimeSpan.FromMinutes(ventanaAutenticacionMinutos),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.OnRejected = async (context, _) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                Math.Ceiling(retryAfter.TotalSeconds).ToString();
        }

        await Results.Problem(
            statusCode: StatusCodes.Status429TooManyRequests,
            title: "Demasiadas solicitudes",
            detail: "Esperá unos instantes antes de volver a intentar.",
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = context.HttpContext.TraceIdentifier
            }).ExecuteAsync(context.HttpContext);
    };
});



builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IVarianteService, VarianteService>();
builder.Services.AddScoped<IOrdenService, OrdenService>();
builder.Services.AddScoped<IEstadisticaService, EstadisticaService>();
builder.Services.AddScoped<IImagenService, ImagenService>();

var proveedorImagenes = builder.Configuration["ImageStorage:Provider"] ?? "Local";
if (proveedorImagenes.Equals("Local", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IAlmacenamientoImagenes, AlmacenamientoImagenesLocal>();
}
else
{
    throw new InvalidOperationException(
        $"El proveedor de imágenes '{proveedorImagenes}' no está registrado. Implementá IAlmacenamientoImagenes y registralo en Program.cs.");
}

builder.Services.AddCors(options =>
{
    var origenesPermitidos = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()?
        .Where(origen => !string.IsNullOrWhiteSpace(origen))
        .Select(origen => origen.Trim().TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray() ?? [];

    if (origenesPermitidos.Length == 0)
    {
        throw new InvalidOperationException(
            "Falta Cors:AllowedOrigins. Configurá al menos el origen del frontend.");
    }

    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirAngular");

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();

if (!app.Environment.IsEnvironment("Testing"))
{
    await DbInitializer.SeedAsync(app.Services, app.Configuration, app.Environment);
}

app.Run();

public partial class Program;
