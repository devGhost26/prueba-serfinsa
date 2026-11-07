using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace GestionProductos.Tests.Infrastructure;

// Base de datos propia para las pruebas: se crea con el mismo database/script.sql que usa la
// aplicación, pero con otro nombre, y se borra al terminar. Así las pruebas no tocan tus datos.
//
// Por defecto usa SQL Server LocalDB. Para usar otro servidor, por ejemplo el de Docker:
//   TEST_SQLSERVER="Server=localhost,1433;User Id=sa;Password=...;TrustServerCertificate=True"
public sealed partial class TestDatabase : IAsyncDisposable
{
    private const string DefaultServer = @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";

    private readonly string _serverConnectionString;

    public string Name { get; } = $"GestionProductos_Tests_{Guid.NewGuid():N}"[..36];

    public string ConnectionString { get; }

    private TestDatabase(string serverConnectionString)
    {
        _serverConnectionString = serverConnectionString;
        ConnectionString = new SqlConnectionStringBuilder(serverConnectionString) { InitialCatalog = Name }.ConnectionString;
    }

    public static async Task<TestDatabase> CreateAsync()
    {
        var server = Environment.GetEnvironmentVariable("TEST_SQLSERVER") ?? DefaultServer;
        var database = new TestDatabase(server);
        await database.RunScriptAsync();
        return database;
    }

    private async Task RunScriptAsync()
    {
        // El script crea "GestionProductos"; aquí se cambia por el nombre de la base de pruebas.
        var script = DatabaseName().Replace(await File.ReadAllTextAsync(FindScript()), Name);

        await using var connection = new SqlConnection(_serverConnectionString);
        await connection.OpenAsync();
        foreach (var batch in GoSeparator().Split(script).Where(b => !string.IsNullOrWhiteSpace(b)))
        {
            await using var command = new SqlCommand(batch, connection) { CommandTimeout = 120 };
            await command.ExecuteNonQueryAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        SqlConnection.ClearAllPools();
        await using var connection = new SqlConnection(_serverConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            $"ALTER DATABASE [{Name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{Name}];", connection);
        await command.ExecuteNonQueryAsync();
    }

    // Busca database/script.sql subiendo desde la carpeta donde se compilaron las pruebas.
    private static string FindScript()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var script = Path.Combine(directory.FullName, "database", "script.sql");
            if (File.Exists(script))
                return script;
        }

        throw new FileNotFoundException("No se encontró database/script.sql en ninguna carpeta superior.");
    }

    [GeneratedRegex(@"\bGestionProductos\b")]
    private static partial Regex DatabaseName();

    // Los "GO" de sqlcmd no son SQL: separan lotes que se ejecutan uno por uno.
    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoSeparator();
}
