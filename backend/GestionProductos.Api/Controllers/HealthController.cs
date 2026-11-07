using GestionProductos.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace GestionProductos.Api.Controllers;

// Permite comprobar que la API está en línea y que llega a la base de datos.
[ApiController]
[Route("api/[controller]")]
public class HealthController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var baseDeDatos = await context.Database.CanConnectAsync(cancellationToken);
        return Ok(new { estado = "en línea", baseDeDatos = baseDeDatos ? "conectada" : "sin conexión" });
    }
}
