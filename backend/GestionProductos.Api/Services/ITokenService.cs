using GestionProductos.Api.Models;

namespace GestionProductos.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
