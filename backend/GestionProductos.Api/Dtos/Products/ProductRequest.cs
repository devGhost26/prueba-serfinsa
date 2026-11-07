using System.ComponentModel.DataAnnotations;

namespace GestionProductos.Api.Dtos.Products;

// Datos para crear o editar un producto. Los números son anulables para que [Required]
// detecte cuando no se envían, en lugar de recibirlos como 0.
public record ProductRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    string Name,

    [StringLength(500, ErrorMessage = "La descripción no puede pasar de 500 caracteres.")]
    string? Description,

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true, ErrorMessage = "El precio debe ser mayor que 0.")]
    decimal? Price,

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    int? Stock,

    [Required(ErrorMessage = "El tipo de producto es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de producto no es válido.")]
    int? ProductTypeId);
