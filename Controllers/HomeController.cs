using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Zlora.Data;
using Zlora.Models;

namespace Zlora.Controllers;

public class HomeController(ContextMongoDb db, UserManager<ApplicationUser> users) : Controller
{
    public IActionResult Index() => View();

    [Authorize(Roles = "Administrador,Secretaria,Profissional")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Painel()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Contas");
        var empresa = user.EmpresaId;
        ViewBag.TotalClientes = await db.Clientes.CountDocumentsAsync(c => c.EmpresaId == empresa);
        ViewBag.TotalProfissionais = await db.Profissionais.CountDocumentsAsync(p => p.EmpresaId == empresa && p.Status == "ativo");
        ViewBag.TotalServicos = await db.Servicos.CountDocumentsAsync(s => s.EmpresaId == empresa && s.Status == "ativo");
        ViewBag.TotalAgendamentos = await db.Agendamentos.CountDocumentsAsync(a => a.EmpresaId == empresa);
        ViewBag.Proximos = await db.Agendamentos.Find(a => a.EmpresaId == empresa && a.Status == "agendado" && a.DataHoraInicio >= DateTime.Now).SortBy(a => a.DataHoraInicio).Limit(5).ToListAsync();
        ViewBag.Clientes = await db.Clientes.Find(c => c.EmpresaId == empresa).ToListAsync();
        ViewBag.Servicos = await db.Servicos.Find(s => s.EmpresaId == empresa).ToListAsync();
        ViewData["Section"] = "VISÃO GERAL";
        return View();
    }

    public IActionResult Erro() { Response.StatusCode = 500; return View(); }
}
