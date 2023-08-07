using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "O nome deve ser informado!")]
    public string Name { get; set; }

    [Required(ErrorMessage = "O E-mail deve ser informado!")]
    [EmailAddress(ErrorMessage = "O E-email é inválido")]
    public string Email { get; set; }
}
