namespace GestionProductos.Api.Services;

// Encripta las contraseñas con BCrypt. Cada hash lleva su propia sal, así que dos
// usuarios con la misma contraseña no quedan con el mismo valor guardado.
public class BCryptPasswordHasher : IPasswordHasher
{
    // Costo 11: cada intento tarda lo suficiente para frenar un ataque de fuerza bruta.
    private const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
