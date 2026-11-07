namespace GestionProductos.Api.Dtos.Auth;

public record LoginResponse(string Token, DateTime ExpiresAt, string Name, string Email, string Role);
