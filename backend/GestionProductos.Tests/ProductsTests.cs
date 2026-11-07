using System.Net;
using System.Net.Http.Json;
using GestionProductos.Api.Dtos.Products;
using GestionProductos.Api.Dtos.ProductTypes;
using GestionProductos.Tests.Infrastructure;

namespace GestionProductos.Tests;

[Collection(ApiCollection.Name)]
public class ProductsTests(ApiFactory factory)
{
    // Tipo "Electrónica" y "Oficina" del script de datos iniciales.
    private const int ElectronicsTypeId = 1;
    private const int OfficeTypeId = 5;

    private static object ValidProduct(string name = "Producto de prueba") =>
        new { name, description = "Creado por las pruebas", price = 19.99m, stock = 7, productTypeId = ElectronicsTypeId };

    private static async Task<ProductResponse> CreateAsync(HttpClient admin, object product)
    {
        var response = await admin.PostAsJsonAsync("/api/products", product);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    // ---------- Acceso ----------

    [Fact]
    public async Task List_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_AsUser_ReturnsSeededProducts()
    {
        var user = await factory.CreateUserClientAsync();

        var products = await user.GetFromJsonAsync<List<ProductResponse>>("/api/products");

        Assert.NotNull(products);
        Assert.Contains(products, p => p.Name == "Teclado mecánico" && p.ProductTypeName == "Electrónica");
    }

    [Fact]
    public async Task WriteOperations_AsUser_Return403()
    {
        var user = await factory.CreateUserClientAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await user.PostAsJsonAsync("/api/products", ValidProduct())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await user.PutAsJsonAsync("/api/products/1", ValidProduct())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await user.DeleteAsync("/api/products/1")).StatusCode);

        // Y el producto sigue ahí.
        Assert.Equal(HttpStatusCode.OK, (await user.GetAsync("/api/products/1")).StatusCode);
    }

    // ---------- CRUD como administrador ----------

    [Fact]
    public async Task Create_AsAdmin_Returns201AndTheProduct()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/products",
            new { name = "  Mouse óptico  ", description = "   ", price = 15.5m, stock = 20, productTypeId = ElectronicsTypeId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        Assert.Equal($"/api/products/{created.Id}", response.Headers.Location?.AbsolutePath);
        Assert.Equal("Mouse óptico", created.Name);
        Assert.Null(created.Description);
        Assert.Equal(15.5m, created.Price);
        Assert.Equal(20, created.Stock);
        Assert.Equal("Electrónica", created.ProductTypeName);
        Assert.Equal(DateTimeKind.Utc, created.CreatedAt.Kind);
    }

    [Fact]
    public async Task GetById_ReturnsTheCreatedProduct()
    {
        var admin = await factory.CreateAdminClientAsync();
        var created = await CreateAsync(admin, ValidProduct("Para consultar"));

        var found = await (await factory.CreateUserClientAsync())
            .GetFromJsonAsync<ProductResponse>($"/api/products/{created.Id}");

        Assert.Equal(created, found);
    }

    [Fact]
    public async Task Update_AsAdmin_SavesTheChanges()
    {
        var admin = await factory.CreateAdminClientAsync();
        var created = await CreateAsync(admin, ValidProduct("Para editar"));

        var response = await admin.PutAsJsonAsync($"/api/products/{created.Id}",
            new { name = "Editado", description = "Nueva descripción", price = 25m, stock = 0, productTypeId = OfficeTypeId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await admin.GetFromJsonAsync<ProductResponse>($"/api/products/{created.Id}");
        Assert.NotNull(updated);
        Assert.Equal("Editado", updated.Name);
        Assert.Equal("Nueva descripción", updated.Description);
        Assert.Equal(25m, updated.Price);
        Assert.Equal(0, updated.Stock);
        Assert.Equal("Oficina", updated.ProductTypeName);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task Delete_AsAdmin_RemovesTheProduct()
    {
        var admin = await factory.CreateAdminClientAsync();
        var created = await CreateAsync(admin, ValidProduct("Para eliminar"));

        var response = await admin.DeleteAsync($"/api/products/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/products/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.DeleteAsync($"/api/products/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task OperationsOnMissingProduct_Return404()
    {
        var admin = await factory.CreateAdminClientAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync("/api/products/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.PutAsJsonAsync("/api/products/999999", ValidProduct())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.DeleteAsync("/api/products/999999")).StatusCode);
    }

    // ---------- Validaciones ----------

    [Fact]
    public async Task Create_WithEmptyBody_ReturnsErrorsForEveryRequiredField()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/products", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fields = (await response.ReadProblemAsync()).ErrorFields();
        Assert.Equivalent(new[] { "Name", "Price", "Stock", "ProductTypeId" }, fields);
    }

    [Theory]
    [InlineData(0, 1, "Price")]       // precio en cero
    [InlineData(-5, 1, "Price")]      // precio negativo
    [InlineData(1.005, 1, "Price")]   // más de 2 decimales
    [InlineData(10, -1, "Stock")]     // stock negativo
    public async Task Create_WithInvalidNumbers_Returns400(decimal price, int stock, string field)
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/products",
            new { name = "Inválido", price, stock, productTypeId = ElectronicsTypeId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(field, (await response.ReadProblemAsync()).ErrorFields());
    }

    [Fact]
    public async Task Create_WithUnknownProductType_Returns400()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/products",
            new { name = "Sin tipo", price = 1m, stock = 1, productTypeId = 999 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ProductTypeId", (await response.ReadProblemAsync()).ErrorFields());
    }

    [Fact]
    public async Task Create_WithTooLongName_Returns400()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/products",
            new { name = new string('a', 101), price = 1m, stock = 1, productTypeId = ElectronicsTypeId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Name", (await response.ReadProblemAsync()).ErrorFields());
    }

    [Fact]
    public async Task Create_WithMalformedJson_Returns400WithoutInternalDetails()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsync("/api/products",
            new StringContent("{\"name\":", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("GestionProductos.Api", body);
    }

    // ---------- Tipos de producto ----------

    [Fact]
    public async Task ProductTypes_ReturnsTheSeededTypesSortedByName()
    {
        var user = await factory.CreateUserClientAsync();

        var types = await user.GetFromJsonAsync<List<ProductTypeResponse>>("/api/product-types");

        Assert.NotNull(types);
        Assert.Equal(["Alimentos", "Electrónica", "Hogar", "Oficina", "Ropa"], types.Select(t => t.Name));
    }

    [Fact]
    public async Task ProductTypes_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/product-types");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
