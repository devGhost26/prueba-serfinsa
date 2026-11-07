namespace GestionProductos.Api.Models;

public class ProductType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = [];
}
