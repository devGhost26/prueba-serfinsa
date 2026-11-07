using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace GestionProductos.Api.Errors;

// Pone en español el título de las respuestas de error que arma ASP.NET por su cuenta
// (sin token, sin permiso, error inesperado...). Los títulos propios, como
// «Producto no encontrado», se dejan como están.
public static class ProblemDetailsTitles
{
    private static readonly Dictionary<int, string> Titles = new()
    {
        [StatusCodes.Status400BadRequest] = "Hay datos inválidos en la solicitud",
        [StatusCodes.Status401Unauthorized] = "Necesitas iniciar sesión",
        [StatusCodes.Status403Forbidden] = "No tienes permiso para esta acción",
        [StatusCodes.Status404NotFound] = "Recurso no encontrado",
        [StatusCodes.Status405MethodNotAllowed] = "Método no permitido",
        [StatusCodes.Status415UnsupportedMediaType] = "Formato de contenido no admitido",
        [StatusCodes.Status429TooManyRequests] = "Demasiados intentos",
        [StatusCodes.Status500InternalServerError] = "Ocurrió un error inesperado",
    };

    // Los títulos que ASP.NET pone por defecto, en inglés.
    private static readonly HashSet<string> DefaultTitles =
    [
        "Bad Request", "One or more validation errors occurred.", "Unauthorized", "Forbidden",
        "Not Found", "Method Not Allowed", "Unsupported Media Type", "Too Many Requests",
        "An error occurred while processing your request.", "Internal Server Error",
    ];

    public static void Apply(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        var status = problem.Status ?? context.HttpContext.Response.StatusCode;

        if ((problem.Title is null || DefaultTitles.Contains(problem.Title)) && Titles.TryGetValue(status, out var title))
            problem.Title = title;

        // Un error inesperado nunca devuelve el detalle de la excepción: solo el traceId para buscarlo en los logs.
        if (status >= StatusCodes.Status500InternalServerError)
            problem.Detail = "Vuelve a intentarlo. Si el problema sigue, informa este código: " +
                (context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity.Id ?? context.HttpContext.TraceIdentifier);
    }
}
