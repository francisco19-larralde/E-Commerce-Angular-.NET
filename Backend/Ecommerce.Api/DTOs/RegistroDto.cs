using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs;

public class RegistroDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public required string Nombre { get; set; }

    [Required(ErrorMessage = "El apellido es obligatorio")]
    public required string Apellido { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "La contraseña debe incluir mayúscula, minúscula, número y símbolo")]
    public required string Password { get; set; }
}
