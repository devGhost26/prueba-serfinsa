using System.Linq.Expressions;
using GestionProductos.Api.Data;
using GestionProductos.Api.Dtos.Products;
using GestionProductos.Api.Models;
using GestionProductos.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionProductos.Api.Controllers;

// Cualquier usuario con sesión puede consultar; solo el rol Admin puede crear, editar y eliminar.
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ProductsController(AppDbContext context) : ControllerBase
{
    // La consulta arma la respuesta en SQL: trae solo las columnas necesarias y el tipo con un JOIN.
    private static readonly Expression<Func<Product, ProductResponse>> ToResponse = p => new ProductResponse(
        p.Id, p.Name, p.Description, p.Price, p.Stock, p.ProductTypeId, p.ProductType.Name, p.CreatedAt, p.UpdatedAt);

    [HttpGet]
    [ProducesResponseType<List<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponse>>> GetAll(CancellationToken cancellationToken) =>
        await context.Products
            .OrderBy(p => p.Name)
            .Select(ToResponse)
            .ToListAsync(cancellationToken);

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await FindResponseAsync(id, cancellationToken);
        return product is null ? ProductNotFound(id) : product;
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken cancellationToken)
    {
        if (await ValidateAsync(request, cancellationToken) is { } invalid)
            return invalid;

        var product = new Product();
        Apply(product, request);
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        var created = await FindResponseAsync(product.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([id], cancellationToken);
        if (product is null)
            return ProductNotFound(id);

        if (await ValidateAsync(request, cancellationToken) is { } invalid)
            return invalid;

        Apply(product, request);
        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return (await FindResponseAsync(id, cancellationToken))!;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await context.Products.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? ProductNotFound(id) : NoContent();
    }

    private Task<ProductResponse?> FindResponseAsync(int id, CancellationToken cancellationToken) =>
        context.Products.Where(p => p.Id == id).Select(ToResponse).SingleOrDefaultAsync(cancellationToken);

    // Las reglas que los atributos del request no cubren: los centavos y que el tipo exista.
    private async Task<ActionResult?> ValidateAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        if (decimal.Round(request.Price!.Value, 2) != request.Price.Value)
            ModelState.AddModelError(nameof(request.Price), "El precio admite como máximo 2 decimales.");

        if (!await context.ProductTypes.AnyAsync(t => t.Id == request.ProductTypeId, cancellationToken))
            ModelState.AddModelError(nameof(request.ProductTypeId), "El tipo de producto no existe.");

        return ModelState.IsValid ? null : ValidationProblem(ModelState);
    }

    private static void Apply(Product product, ProductRequest request)
    {
        product.Name = request.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        product.Price = request.Price!.Value;
        product.Stock = request.Stock!.Value;
        product.ProductTypeId = request.ProductTypeId!.Value;
    }

    private ObjectResult ProductNotFound(int id) => Problem(
        title: "Producto no encontrado",
        detail: $"No existe un producto con el id {id}.",
        statusCode: StatusCodes.Status404NotFound);
}
