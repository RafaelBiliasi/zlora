using System.ComponentModel.DataAnnotations;
namespace Zlora.ViewModels;

public class EsqueciSenhaViewModel
{
    [Required(ErrorMessage = "Informe seu e-mail."), EmailAddress(ErrorMessage = "Informe um e-mail válido."), StringLength(254)]
    public string Email { get; set; } = "";
}

public class RedefinirSenhaViewModel
{
    [Required] public string UserId { get; set; } = "";
    [Required] public string Token { get; set; } = "";
    [Required(ErrorMessage = "Informe a nova senha."), StringLength(15, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 15 caracteres."), DataType(DataType.Password)]
    public string NovaSenha { get; set; } = "";
    [Required(ErrorMessage = "Confirme a nova senha."), Compare(nameof(NovaSenha), ErrorMessage = "As senhas não coincidem."), DataType(DataType.Password)]
    public string ConfirmarSenha { get; set; } = "";
}
