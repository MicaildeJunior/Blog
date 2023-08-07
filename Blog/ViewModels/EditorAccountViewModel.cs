using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels;

public class EditorAccountViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "Este campo deve conter entre 3 e 40 caracteres")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "O E-mail é obrigatório")]
    public string? Email { get; set; }
}
