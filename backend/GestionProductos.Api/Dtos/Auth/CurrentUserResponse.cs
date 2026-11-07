namespace GestionProductos.Api.Dtos.Auth;

public record CurrentUserResponse(string Id, string Name, string Email, string Role);
