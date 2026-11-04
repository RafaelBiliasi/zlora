using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Zlora.Models;
using Zlora.ViewModels;
using Zlora.Data;
using Zlora.Services;
using System.Security.Cryptography;
using System.Text;

namespace Zlora.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ContasController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    RoleManager<ApplicationRole> roles, ContextMongoDb context, EmailService emailService,
    ILogger<ContasController> logger) : Controller
{
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user != null)
        {
            var result = await signIn.CheckPasswordSignInAsync(user, model.Senha, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                if (user.Status != "ativo" || !(await users.GetRolesAsync(user)).Any())
                {
                    ModelState.AddModelError("", "Seu acesso não está ativo. Procure o administrador do estabelecimento.");
                    return View(model);
                }
                await context.Usuarios.UpdateOneAsync(u => u.Id == user.Id,
                    Builders<ApplicationUser>.Update.Set(u => u.UltimoAcesso, DateTime.UtcNow));
                await signIn.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Painel", "Home");
            }
        }
        ModelState.AddModelError("", "E-mail ou senha inválidos. Após cinco tentativas, aguarde cinco minutos.");
        return View(model);
    }

    public IActionResult Registro() => View(new RegistroViewModel());

    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        model.Nome = (model.Nome ?? "").Trim(); model.NomeContato = (model.NomeContato ?? "").Trim();
        if (model.Nome.Length < 2 || model.NomeContato.Length < 2) ModelState.AddModelError("", "Informe os nomes do estabelecimento e do contato.");
        var telefone = new string((model.Telefone ?? "").Where(char.IsDigit).ToArray());
        if (telefone.Length < 10 || telefone.Length > 11) ModelState.AddModelError("", "Informe um telefone com DDD e 10 ou 11 dígitos.");
        if (!ModelState.IsValid) return View(model);
        var empresa = new Empresa { NomeFantasia = model.Nome };
        var user = new ApplicationUser {
            Nome = model.NomeContato, Estabelecimento = model.Nome, PhoneNumber = telefone,
            UserName = model.Email.Trim(), Email = model.Email.Trim(), Status = "ativo", EmpresaId = empresa.Id
        };
        user.Roles.Add((await roles.FindByNameAsync(Perfis.Administrador))!.Id);
        empresa.ProprietarioUsuarioId = user.Id;
        await context.Empresas.InsertOneAsync(empresa);
        try
        {
            var result = await users.CreateAsync(user, model.Senha);
            if (result.Succeeded)
            {
                await context.Profissionais.InsertOneAsync(new Profissional { EmpresaId = empresa.Id, UsuarioId = user.Id, NomeExibicao = user.Nome, Horario = "08:00 - 18:00" });
                TempData["Mensagem"] = "Estabelecimento cadastrado. Entre como administrador para cadastrar sua equipe.";
                return RedirectToAction(nameof(Login));
            }
            ModelState.AddModelError("", "Não foi possível cadastrar. Verifique os dados; o e-mail pode já estar cadastrado.");
        }
        catch (MongoWriteException) { ModelState.AddModelError("", "Não foi possível cadastrar. Verifique se o e-mail já está cadastrado."); }
        await context.Empresas.DeleteOneAsync(e => e.Id == empresa.Id);
        return View(model);
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult EsqueciSenha() => View(new EsqueciSenhaViewModel());

    [HttpPost]
    public async Task<IActionResult> EsqueciSenha(EsqueciSenhaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        // A mesma mensagem para qualquer e-mail; não revela quem possui conta.
        if (!emailService.Configurado)
        {
            ModelState.AddModelError("", "O envio de e-mail ainda não foi configurado. Entre em contato com o responsável pelo sistema.");
            return View(model);
        }
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user != null && user.Status == "ativo")
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var hash = TokenHash(token);
            var limite = DateTime.UtcNow.AddMinutes(-1);
            // No máximo um envio por minuto por conta, inclusive entre navegadores diferentes.
            var result = await context.Usuarios.UpdateOneAsync(u => u.Id == user.Id &&
                (u.RecuperacaoEnviadaEm == null || u.RecuperacaoEnviadaEm < limite),
                Builders<ApplicationUser>.Update.Set(u => u.RecuperacaoHash, hash)
                    .Set(u => u.RecuperacaoEnviadaEm, DateTime.UtcNow)
                    .Set(u => u.RecuperacaoExpiraEm, DateTime.UtcNow.AddMinutes(30)));
            if (result.ModifiedCount == 1)
            {
                var link = emailService.Settings.BaseUrl.TrimEnd('/') + Url.Action(nameof(RedefinirSenha), "Contas", new { userId = user.Id, token });
                try
                {
                    await emailService.SendEmailAsync(user.Email!, "Zlora — redefinição de senha",
                        "Recebemos um pedido para redefinir sua senha na Zlora.\n\nAbra o link abaixo em até 30 minutos:\n" + link +
                        "\n\nO link pode ser usado uma única vez. Se você não fez este pedido, ignore este e-mail. Sua senha permanece a mesma.");
                }
                catch (Exception ex)
                {
                    // Não registra endereço, senha SMTP ou token de recuperação.
                    logger.LogError("Falha no envio de recuperação: {Tipo}", ex.GetType().Name);
                    await context.Usuarios.UpdateOneAsync(u => u.Id == user.Id && u.RecuperacaoHash == hash,
                        Builders<ApplicationUser>.Update.Set(u => u.RecuperacaoHash, null));
                }
            }
        }
        return RedirectToAction(nameof(RecuperacaoEnviada));
    }

    public IActionResult RecuperacaoEnviada() => View();

    public IActionResult RedefinirSenha(string? userId, string? token)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        return View(new RedefinirSenhaViewModel { UserId = userId ?? "", Token = token ?? "" });
    }

    [HttpPost]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByIdAsync(model.UserId);
        if (user != null && user.Status == "ativo" && await users.VerifyUserTokenAsync(user,
            users.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", model.Token))
        {
            var hash = TokenHash(model.Token);
            // Consome de forma atômica para impedir reutilização simultânea do mesmo link.
            var consumed = await context.Usuarios.UpdateOneAsync(u => u.Id == user.Id && u.RecuperacaoHash == hash && u.RecuperacaoExpiraEm > DateTime.UtcNow,
                Builders<ApplicationUser>.Update.Set(u => u.RecuperacaoHash, null));
            if (consumed.ModifiedCount == 1)
            {
                user.RecuperacaoHash = null;
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                user.AlteradoEm = DateTime.UtcNow;
                var result = await users.ResetPasswordAsync(user, model.Token, model.NovaSenha);
                if (result.Succeeded)
                {
                    await signIn.SignOutAsync();
                    TempData["Mensagem"] = "Senha redefinida com sucesso. Entre com sua nova senha.";
                    return RedirectToAction(nameof(Login));
                }
            }
        }
        ModelState.AddModelError("", "Este link é inválido, expirou ou já foi utilizado. Solicite outro link.");
        return View(model);
    }

    private static string TokenHash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public IActionResult AcessoNegado() { Response.StatusCode = 403; return View(); }
}
