using System.ComponentModel.DataAnnotations;

namespace MedVoll.Web.Dtos;

public class UsuarioDto
{
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "O e-mail inserido não é válido.")]  
    public string? Email { get; set; }

    [RegularExpression(@"^(?=.*[0-9])(?=.*[!@#$%^&*(),.?\:{}|<>]).{6,}$",
        ErrorMessage = "A senha deve conter ao menos um número e um caractere especial.")] 
    public string? Senha { get; set; }
}
