using Microsoft.AspNetCore.Mvc;

namespace GestionProductos.Api.Controllers;

// Permite comprobar que la API está en línea.
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { estado = "en línea" });
}
