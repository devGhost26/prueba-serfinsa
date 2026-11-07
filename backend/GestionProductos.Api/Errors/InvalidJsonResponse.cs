using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GestionProductos.Api.Errors;

// Cuando el cuerpo no es un JSON válido, responde un solo mensaje claro en lugar de los
// errores sueltos, y en inglés, que arma el lector de JSON.
public static class InvalidJsonResponse
{
    // El lector de JSON registra sus errores con la ruta del JSON ("$.name") o sin clave si el cuerpo vino vacío.
    public static bool IsInvalidJson(ModelStateDictionary modelState) =>
        modelState.Keys.Any(key => key.Length == 0 || key.StartsWith('$'));

    public static ObjectResult Create(HttpContext httpContext)
    {
        var problem = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>().CreateProblemDetails(
            httpContext,
            StatusCodes.Status400BadRequest,
            detail: "El cuerpo de la solicitud no es un JSON válido.");

        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" },
        };
    }
}
