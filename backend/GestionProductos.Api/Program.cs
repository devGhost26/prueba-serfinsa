using System.Text;
using System.Threading.RateLimiting;
using GestionProductos.Api.Data;
using GestionProductos.Api.Errors;
using GestionProductos.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// Si falta la clave del token o es muy corta, la API no arranca.
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = AuthClaims.Name,
            RoleClaimType = AuthClaims.Role,
        };
    });
builder.Services.AddAuthorization();

// Todo error sale en formato ProblemDetails (RFC 9457) y con el título en español.
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsTitles.Apply);

// Solo el frontend puede llamar a la API desde el navegador. Los orígenes salen de appsettings.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Location")));

// Intentos de inicio de sesión por minuto desde la misma IP (5 por defecto), contra la fuerza bruta.
var loginPermitLimit = builder.Configuration.GetValue("RateLimiting:LoginPermitLimit", 5);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(RateLimitPolicies.Login, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = loginPermitLimit, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers()
    // Un JSON mal formado no devuelve el mensaje interno del lector, que nombra clases del backend.
    .AddJsonOptions(options => options.AllowInputFormatterExceptionMessages = false)
    .ConfigureApiBehaviorOptions(options =>
    {
        var validationResponse = options.InvalidModelStateResponseFactory;
        options.InvalidModelStateResponseFactory = context =>
            InvalidJsonResponse.IsInvalidJson(context.ModelState)
                ? InvalidJsonResponse.Create(context.HttpContext)
                : validationResponse(context);
    });
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// En producción todo va por HTTPS. En desarrollo no se redirige: el frontend llama por http
// y el navegador bloquea la consulta previa de CORS si recibe una redirección. En Docker se
// apaga con Https__Redirect=false, porque ahí el HTTPS lo pondría un proxy delante.
if (!app.Environment.IsDevelopment() && app.Configuration.GetValue("Https:Redirect", true))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseRateLimiter();

// Primero se identifica al usuario con su token y después se revisan sus permisos.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Permite que las pruebas de integración levanten la API con WebApplicationFactory.
public partial class Program;
