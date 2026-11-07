using GestionProductos.Api.Data;
using GestionProductos.Api.Dtos.ProductTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionProductos.Api.Controllers;

// Los tipos de producto, para el selector del formulario.
[ApiController]
[Route("api/product-types")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ProductTypesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<ProductTypeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductTypeResponse>>> GetAll(CancellationToken cancellationToken) =>
        await context.ProductTypes
            .OrderBy(t => t.Name)
            .Select(t => new ProductTypeResponse(t.Id, t.Name))
            .ToListAsync(cancellationToken);
}
