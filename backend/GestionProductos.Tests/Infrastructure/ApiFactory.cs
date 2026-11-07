using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestionProductos.Api.Dtos.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionProductos.Tests.Infrastructure;

// Levanta la API completa en memoria, conectada a una base de pruebas propia en SQL Server.
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@demo.com";
    public const string AdminPassword = "Admin123!";
    public const string UserEmail = "usuario@demo.com";
    public const string UserPassword = "Usuario123!";

    private TestDatabase? _database;

    // Las pruebas inician sesión muchas veces; con el límite real de 5 por minuto recibirían 429.
    protected virtual int LoginPermitLimit => 1000;

    public async Task InitializeAsync() => _database = await TestDatabase.CreateAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _database!.ConnectionString);
        builder.UseSetting("Jwt:Key", "clave-solo-para-las-pruebas-de-integracion-0123456789");
        builder.UseSetting("RateLimiting:LoginPermitLimit", LoginPermitLimit.ToString());
        builder.UseSetting("Https:Redirect", "false");
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var response = await CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.Token;
    }

    public async Task<HttpClient> CreateClientAsAsync(string email, string password)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(email, password));
        return client;
    }

    public Task<HttpClient> CreateAdminClientAsync() => CreateClientAsAsync(AdminEmail, AdminPassword);

    public Task<HttpClient> CreateUserClientAsync() => CreateClientAsAsync(UserEmail, UserPassword);

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        if (_database is not null)
            await _database.DisposeAsync();
    }
}

// La misma API con el límite de intentos de login bajo, para probarlo sin hacer cientos de peticiones.
public class LimitedLoginApiFactory : ApiFactory
{
    public const int Limit = 3;

    protected override int LoginPermitLimit => Limit;
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "API";
}
