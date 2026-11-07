using System.Text.Json;

namespace GestionProductos.Tests.Infrastructure;

public static class ProblemDetailsExtensions
{
    // Lee el ProblemDetails de una respuesta de error.
    public static async Task<JsonElement> ReadProblemAsync(this HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    // Los nombres de los campos con error de validación: { "errors": { "Name": [...] } }.
    public static IReadOnlyCollection<string> ErrorFields(this JsonElement problem) =>
        problem.GetProperty("errors").EnumerateObject().Select(p => p.Name).ToArray();
}
