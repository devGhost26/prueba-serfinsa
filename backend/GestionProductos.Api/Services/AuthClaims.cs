namespace GestionProductos.Api.Services;

// Nombres de los claims del token. El mismo nombre se usa al crearlo y al validarlo,
// para que el rol del token sea el que revisa [Authorize(Roles = ...)].
public static class AuthClaims
{
    public const string Role = "role";
    public const string Name = "name";
}
