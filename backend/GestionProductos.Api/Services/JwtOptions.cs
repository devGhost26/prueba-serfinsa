using System.ComponentModel.DataAnnotations;

namespace GestionProductos.Api.Services;

// Configuración del token, sección "Jwt" de appsettings. La clave no va en appsettings.json:
// en desarrollo está en appsettings.Development.json y en producción va por variable de entorno.
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    // HMAC-SHA256 necesita al menos 256 bits: 32 caracteres como mínimo.
    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; } = 60;
}
