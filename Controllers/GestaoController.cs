using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Zlora.Data;
using Zlora.Models;

namespace Zlora.Controllers;

[Authorize]
public class GestaoController(ContextMongoDb db, UserManager<ApplicationUser> users) : Controller
{
    private async Task<ApplicationUser> Usuario() => (await users.GetUserAsync(User))!;
    private bool PodeGerenciar() => User.IsInRole(Perfis.Administrador) || User.IsInRole(Perfis.Secretaria);
    private async Task<Profissional?> ProfissionalAtual(ApplicationUser u) => await db.Profissionais.Find(p => p.EmpresaId == u.EmpresaId && (p.UsuarioId == u.Id || p.NomeExibicao == u.Nome)).FirstOrDefaultAsync();

    public async Task<IActionResult> Clientes()
    {
        var u = await Usuario(); return View(await db.Clientes.Find(c => c.EmpresaId == u.EmpresaId).SortBy(c => c.Nome).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CriarCliente(Cliente model)
    {
        if (!PodeGerenciar()) return Forbid(); model.EmpresaId = (await Usuario()).EmpresaId; model.Id = Guid.NewGuid().ToString();
        await db.Clientes.InsertOneAsync(model); TempData["Mensagem"] = "Cliente cadastrado com sucesso."; return RedirectToAction(nameof(Clientes));
    }

    public async Task<IActionResult> Servicos()
    {
        var u = await Usuario(); var servicos = await db.Servicos.Find(s => s.EmpresaId == u.EmpresaId).SortBy(s => s.Nome).ToListAsync();
        ViewBag.Profissionais = await db.Profissionais.Find(p => p.EmpresaId == u.EmpresaId && p.Status == "ativo").ToListAsync(); return View(servicos);
    }

    [HttpPost]
    public async Task<IActionResult> CriarServico(Servico model)
    {
        if (!User.IsInRole(Perfis.Administrador)) return Forbid(); model.EmpresaId = (await Usuario()).EmpresaId; model.Id = Guid.NewGuid().ToString();
        await db.Servicos.InsertOneAsync(model); TempData["Mensagem"] = "Serviço criado. O link público já está disponível."; return RedirectToAction(nameof(Servicos));
    }

    public async Task<IActionResult> Agenda()
    {
        var u = await Usuario(); var filtro = Builders<Agendamento>.Filter.Eq(a => a.EmpresaId, u.EmpresaId);
        if (User.IsInRole(Perfis.Profissional)) { var p = await ProfissionalAtual(u); filtro &= Builders<Agendamento>.Filter.Eq(a => a.ProfissionalId, p?.Id ?? "__sem_agenda__"); }
        var agenda = await db.Agendamentos.Find(filtro).SortBy(a => a.DataHoraInicio).ToListAsync();
        ViewBag.Clientes = await db.Clientes.Find(c => c.EmpresaId == u.EmpresaId).SortBy(c => c.Nome).ToListAsync(); ViewBag.Profissionais = await db.Profissionais.Find(p => p.EmpresaId == u.EmpresaId && p.Status == "ativo").ToListAsync(); ViewBag.Servicos = await db.Servicos.Find(s => s.EmpresaId == u.EmpresaId && s.Status == "ativo").ToListAsync();
        return View(agenda);
    }

    [HttpPost]
    public async Task<IActionResult> CriarAgendamento(Agendamento model)
    {
        var u = await Usuario(); if (!PodeGerenciar() && !User.IsInRole(Perfis.Profissional)) return Forbid();
        if (User.IsInRole(Perfis.Profissional)) { var p = await ProfissionalAtual(u); if (p == null) return Forbid(); model.ProfissionalId = p.Id; }
        if (model.DataHoraInicio < DateTime.Now.AddHours(24)) { TempData["Erro"] = "O agendamento precisa respeitar 24 horas de antecedência."; return RedirectToAction(nameof(Agenda)); }
        model.EmpresaId = u.EmpresaId; model.Id = Guid.NewGuid().ToString(); model.Status = "agendado"; await db.Agendamentos.InsertOneAsync(model); TempData["Mensagem"] = "Agendamento criado."; return RedirectToAction(nameof(Agenda));
    }

    [HttpPost]
    public async Task<IActionResult> EditarAgendamento(Agendamento model)
    {
        var u = await Usuario(); var atual = await db.Agendamentos.Find(a => a.Id == model.Id && a.EmpresaId == u.EmpresaId).FirstOrDefaultAsync(); if (atual == null) return NotFound();
        var p = await ProfissionalAtual(u); if (User.IsInRole(Perfis.Profissional) && (p == null || atual.ProfissionalId != p.Id)) return Forbid(); if (!PodeGerenciar() && !User.IsInRole(Perfis.Profissional)) return Forbid(); if (User.IsInRole(Perfis.Profissional)) model.ProfissionalId = p!.Id;
        if (model.DataHoraInicio < DateTime.Now.AddHours(24)) { TempData["Erro"] = "A alteração precisa respeitar 24 horas de antecedência."; return RedirectToAction(nameof(Agenda)); }
        await db.Agendamentos.UpdateOneAsync(a => a.Id == model.Id, Builders<Agendamento>.Update.Set(a => a.DataHoraInicio, model.DataHoraInicio).Set(a => a.ProfissionalId, model.ProfissionalId).Set(a => a.Observacoes, model.Observacoes)); TempData["Mensagem"] = "Agendamento atualizado."; return RedirectToAction(nameof(Agenda));
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(string id)
    {
        var u = await Usuario(); var a = await db.Agendamentos.Find(x => x.Id == id && x.EmpresaId == u.EmpresaId).FirstOrDefaultAsync(); if (a == null) return NotFound();
        if (User.IsInRole(Perfis.Profissional)) { var p = await ProfissionalAtual(u); if (p == null || a.ProfissionalId != p.Id) return Forbid(); }
        await db.Agendamentos.UpdateOneAsync(x => x.Id == id, Builders<Agendamento>.Update.Set(x => x.Status, "cancelado")); TempData["Mensagem"] = "Agendamento cancelado."; return RedirectToAction(nameof(Agenda));
    }

    public async Task<IActionResult> Relatorio(string periodo = "mes")
    {
        if (!User.IsInRole(Perfis.Administrador)) return Forbid();
        var u = await Usuario(); var inicio = periodo == "semana" ? DateTime.Today.AddDays(-7) : DateTime.Today.AddDays(-30); var agenda = await db.Agendamentos.Find(a => a.EmpresaId == u.EmpresaId && a.DataHoraInicio >= inicio).SortBy(a => a.DataHoraInicio).ToListAsync();
        ViewBag.Clientes = await db.Clientes.Find(c => c.EmpresaId == u.EmpresaId).ToListAsync(); ViewBag.Servicos = await db.Servicos.Find(s => s.EmpresaId == u.EmpresaId).ToListAsync(); ViewBag.Periodo = periodo; return View(agenda);
    }

    public async Task<IActionResult> RelatorioPdf(string periodo = "mes")
    {
        if (!User.IsInRole(Perfis.Administrador)) return Forbid();
        var u = await Usuario(); var inicio = periodo == "semana" ? DateTime.Today.AddDays(-7) : DateTime.Today.AddDays(-30); var lista = await db.Agendamentos.Find(a => a.EmpresaId == u.EmpresaId && a.DataHoraInicio >= inicio).SortBy(a => a.DataHoraInicio).ToListAsync(); var clientes = await db.Clientes.Find(c => c.EmpresaId == u.EmpresaId).ToListAsync(); var servicos = await db.Servicos.Find(s => s.EmpresaId == u.EmpresaId).ToListAsync();
        var linhas = new List<string> { $"ZLORA - RELATORIO DE AGENDAMENTOS ({periodo.ToUpperInvariant()})", $"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}", "" }; foreach (var item in lista) { var cliente = clientes.FirstOrDefault(c => c.Id == item.ClienteId)?.Nome ?? "Cliente"; var servico = servicos.FirstOrDefault(s => s.Id == item.ServicoId)?.Nome ?? "Servico"; linhas.Add($"{item.DataHoraInicio:dd/MM/yyyy HH:mm} | {cliente} | {servico} | {item.Status}"); } if (lista.Count == 0) linhas.Add("Nenhum agendamento encontrado no periodo.");
        return File(SimplePdf(linhas), "application/pdf", $"zlora-relatorio-{periodo}.pdf");
    }

    private static byte[] SimplePdf(IEnumerable<string> linhas)
    {
        static string Safe(string text) => text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ã", "a").Replace("õ", "o").Replace("ç", "c");
        var content = "BT /F1 10 Tf 45 770 Td 0 -16 Td " + string.Join(" Tj 0 -16 Td ", linhas.Select(x => $"({Safe(x)})")) + " Tj ET"; var objects = new[] { "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R] /Count 1 >>", "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>", $"<< /Length {content.Length} >>\nstream\n{content}\nendstream" };
        using var stream = new MemoryStream(); using var writer = new StreamWriter(stream, System.Text.Encoding.ASCII, 1024, leaveOpen: true); writer.Write("%PDF-1.4\n"); writer.Flush(); var offsets = new List<long> { 0 }; foreach (var (obj, index) in objects.Select((x, i) => (x, i + 1))) { offsets.Add(stream.Position); writer.Write($"{index} 0 obj\n{obj}\nendobj\n"); writer.Flush(); } var xref = stream.Position; writer.Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n"); foreach (var offset in offsets.Skip(1)) writer.Write($"{offset:0000000000} 00000 n \n"); writer.Write($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF"); writer.Flush(); return stream.ToArray();
    }
}
