using System.ComponentModel.DataAnnotations;

namespace GestionProductos.Api.Dtos.Auth;

public record LoginRequest(
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password);
