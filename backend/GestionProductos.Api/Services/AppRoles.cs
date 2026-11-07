namespace GestionProductos.Api.Services;

// Nombres de los roles, iguales a los de la tabla Roles.
public static class AppRoles
{
    // Puede crear, editar y eliminar productos.
    public const string Admin = "Admin";

    // Solo puede consultar.
    public const string User = "User";
}
