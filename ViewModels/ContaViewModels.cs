using System.ComponentModel.DataAnnotations;

namespace Zlora.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Informe a senha."), DataType(DataType.Password)]
    public string Senha { get; set; } = "";
}

public class RegistroViewModel
{
    [Required, StringLength(150, MinimumLength = 2)] public string Nome { get; set; } = "";
    [Required, StringLength(120, MinimumLength = 2)] public string NomeContato { get; set; } = "";
    [Required, RegularExpression(@"[0-9()\s+\-]{10,20}", ErrorMessage = "Informe um telefone válido com DDD.")]
    public string Telefone { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(15, MinimumLength = 6), DataType(DataType.Password)] public string Senha { get; set; } = "";
}

public class UsuarioViewModel
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Estabelecimento { get; set; } = "";
    public string Perfil { get; set; } = "";
    public string Status { get; set; } = "";
}

public class ColaboradorViewModel
{
    [Required, StringLength(120, MinimumLength = 2)] public string Nome { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, RegularExpression(@"[0-9()\s+\-]{10,20}")] public string Telefone { get; set; } = "";
    [Required, StringLength(15, MinimumLength = 6)] public string Senha { get; set; } = "";
    [Required] public string Perfil { get; set; } = "";
}
