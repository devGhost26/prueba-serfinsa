namespace GestionProductos.Api.Dtos.Products;

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    int ProductTypeId,
    string ProductTypeName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
