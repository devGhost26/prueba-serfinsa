using System.Security.Claims;
using GestionProductos.Api.Data;
using GestionProductos.Api.Dtos.Auth;
using GestionProductos.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GestionProductos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
    : ControllerBase
{
    // Hash de una contraseña cualquiera. Se verifica contra él cuando el correo no existe, para
    // que la respuesta tarde lo mismo y no delate qué correos están registrados.
    private const string DummyHash = "$2a$11$PBa0PCjF3Alr85gRR646lOCihKGh7dCx44f9H6x4WzzOlM//48QEy";

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        var passwordOk = passwordHasher.Verify(request.Password, user?.PasswordHash ?? DummyHash);
        if (user is null || !passwordOk)
        {
            // El mismo mensaje para correo y contraseña: no se dice cuál de los dos falló.
            return Problem(
                title: "Credenciales incorrectas",
                detail: "El correo o la contraseña no son correctos.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var (token, expiresAt) = tokenService.CreateToken(user);
        return Ok(new LoginResponse(token, expiresAt, user.Name, user.Email, user.Role.Name));
    }

    // Devuelve los datos del usuario dueño del token. Sirve para comprobar que la sesión sigue vigente.
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> Me() => Ok(new CurrentUserResponse(
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)!,
        User.Identity!.Name!,
        User.FindFirstValue(JwtRegisteredClaimNames.Email)!,
        User.FindFirstValue(AuthClaims.Role)!));
}
