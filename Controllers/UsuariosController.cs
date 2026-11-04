using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Zlora.Data;
using Zlora.Models;
using Zlora.ViewModels;

namespace Zlora.Controllers;

[Authorize(Roles = Perfis.Administrador)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class UsuariosController(UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles, ContextMongoDb context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var lista = new List<UsuarioViewModel>();
        var admin = await users.GetUserAsync(User);
        foreach (var user in await context.Usuarios.Find(u => u.EmpresaId == admin!.EmpresaId).SortBy(u => u.Nome).ToListAsync())
            lista.Add(new UsuarioViewModel {
                Id = user.Id, Nome = user.Nome, Email = user.Email!, Estabelecimento = user.Estabelecimento,
                Status = user.Status, Perfil = (await users.GetRolesAsync(user)).FirstOrDefault() ?? ""
            });
        return View(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Perfil(string id, string perfil)
    {
        if (perfil != Perfis.Secretaria && perfil != Perfis.Profissional) return BadRequest("Perfil inválido.");
        if (id == users.GetUserId(User))
        {
            TempData["Mensagem"] = "Você não pode alterar o próprio perfil. Solicite a outro administrador.";
            return RedirectToAction(nameof(Index));
        }
        var admin = await users.GetUserAsync(User);
        var user = await context.Usuarios.Find(u => u.Id == id && u.EmpresaId == admin!.EmpresaId).FirstOrDefaultAsync();
        if (user == null || await users.IsInRoleAsync(user, Perfis.Administrador)) return NotFound();
        try
        {
            var role = await roles.FindByNameAsync(perfil);
            if (role == null) return BadRequest("Perfil não encontrado.");
            // MongoDB altera perfil, status e sessão de forma atômica em um documento.
            await context.Usuarios.UpdateOneAsync(u => u.Id == id && u.EmpresaId == admin!.EmpresaId,
                Builders<ApplicationUser>.Update.Set(u => u.Roles, new List<string> { role.Id })
                    .Set(u => u.Status, "ativo").Set(u => u.AlteradoEm, DateTime.UtcNow)
                    .Set(u => u.SecurityStamp, Guid.NewGuid().ToString()));
            TempData["Mensagem"] = "Perfil atualizado. O usuário deverá entrar novamente.";
        }
        catch (MongoException)
        {
            TempData["Mensagem"] = "Não foi possível alterar o perfil. Atualize a página e tente novamente.";
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Criar() => View(new ColaboradorViewModel());

    [HttpPost]
    public async Task<IActionResult> Criar(ColaboradorViewModel model)
    {
        if (model.Perfil != Perfis.Secretaria && model.Perfil != Perfis.Profissional)
            ModelState.AddModelError("", "Escolha Secretaria ou Profissional.");
        model.Nome = (model.Nome ?? "").Trim();
        var telefone = new string((model.Telefone ?? "").Where(char.IsDigit).ToArray());
        if (model.Nome.Length < 2 || telefone.Length < 10 || telefone.Length > 11)
            ModelState.AddModelError("", "Verifique o nome e o telefone com DDD.");
        if (!ModelState.IsValid) return View(model);
        var admin = await users.GetUserAsync(User);
        var user = new ApplicationUser {
            Nome = model.Nome, Email = model.Email.Trim(), UserName = model.Email.Trim(), PhoneNumber = telefone,
            EmpresaId = admin!.EmpresaId, Estabelecimento = admin.Estabelecimento, Status = "ativo"
        };
        user.Roles.Add((await roles.FindByNameAsync(model.Perfil))!.Id);
        try
        {
            var result = await users.CreateAsync(user, model.Senha);
            if (result.Succeeded)
            {
                if (model.Perfil == Perfis.Profissional)
                {
                    await context.Profissionais.InsertOneAsync(new Profissional { EmpresaId = admin.EmpresaId, UsuarioId = user.Id, NomeExibicao = user.Nome });
                }
                TempData["Mensagem"] = "Usuário cadastrado."; return RedirectToAction(nameof(Index));
            }
        }
        catch (MongoWriteException) { }
        ModelState.AddModelError("", "Não foi possível cadastrar. Verifique os dados e se o e-mail já está em uso.");
        return View(model);
    }
}
