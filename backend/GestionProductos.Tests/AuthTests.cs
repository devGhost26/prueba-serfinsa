using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestionProductos.Api.Dtos.Auth;
using GestionProductos.Tests.Infrastructure;

namespace GestionProductos.Tests;

[Collection(ApiCollection.Name)]
public class AuthTests(ApiFactory factory)
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUser()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = ApiFactory.AdminPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.Equal(3, login.Token.Split('.').Length);
        Assert.Equal("Admin", login.Role);
        Assert.Equal(ApiFactory.AdminEmail, login.Email);
        Assert.True(login.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_IgnoresEmailCaseAndSurroundingSpaces()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = "  Admin@Demo.COM ", password = ApiFactory.AdminPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = "incorrecta" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.ReadProblemAsync();
        Assert.Equal("Credenciales incorrectas", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsSameErrorAsWrongPassword()
    {
        var client = factory.CreateClient();
        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = "incorrecta" });
        var unknownEmail = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "nadie@demo.com", password = ApiFactory.AdminPassword });

        // No se debe poder saber qué correos están registrados.
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        Assert.Equal(
            (await wrongPassword.ReadProblemAsync()).GetProperty("detail").GetString(),
            (await unknownEmail.ReadProblemAsync()).GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Login_WithInvalidData_Returns400WithFieldErrors()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = "no-es-un-correo", password = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fields = (await response.ReadProblemAsync()).ErrorFields();
        Assert.Contains("Email", fields);
        Assert.Contains("Password", fields);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsCurrentUser()
    {
        var client = await factory.CreateUserClientAsync();

        var me = await client.GetFromJsonAsync<CurrentUserResponse>("/api/auth/me");

        Assert.NotNull(me);
        Assert.Equal(ApiFactory.UserEmail, me.Email);
        Assert.Equal("User", me.Role);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithTamperedToken_Returns401()
    {
        var token = await factory.LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        var client = factory.CreateClient();
        // Cambiar la firma invalida el token.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^4] + "AAAA");

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithTokenSignedByAnotherKey_Returns401()
    {
        // Un token con el formato correcto pero firmado con otra clave: el clásico token falsificado.
        const string forged =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJzdWIiOiIxIiwicm9sZSI6IkFkbWluIiwiaXNzIjoiR2VzdGlvblByb2R1Y3Rvcy5BcGkiLCJhdWQiOiJHZXN0aW9uUHJvZHVjdG9zLkZyb250ZW5kIiwiZXhwIjo0MTAyNDQ0ODAwfQ." +
            "c2lnbmF0dXJhLWZhbHNhLWZpcm1hZGEtY29uLW90cmEtY2xhdmU";
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", forged);

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class LoginRateLimitTests(LimitedLoginApiFactory factory) : IClassFixture<LimitedLoginApiFactory>
{
    [Fact]
    public async Task Login_AfterTooManyAttempts_Returns429()
    {
        var client = factory.CreateClient();
        var attempt = new { email = ApiFactory.AdminEmail, password = "incorrecta" };

        for (var i = 0; i < LimitedLoginApiFactory.Limit; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", attempt)).StatusCode);

        var blocked = await client.PostAsJsonAsync("/api/auth/login", attempt);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
    }
}
