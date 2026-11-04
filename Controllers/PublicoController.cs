using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Zlora.Data;
using Zlora.Models;

namespace Zlora.Controllers;

public class PublicoController(ContextMongoDb db) : Controller
{
    public async Task<IActionResult> Servico(string id)
    {
        var servico = await db.Servicos.Find(s => s.Id == id && s.Status == "ativo").FirstOrDefaultAsync();
        if (servico == null) return NotFound();
        ViewBag.Profissionais = await db.Profissionais.Find(p => p.EmpresaId == servico.EmpresaId && p.Status == "ativo").ToListAsync();
        return View(servico);
    }

    [HttpPost]
    public async Task<IActionResult> Agendar(string servicoId, string nome, string email, string telefone, DateTime dataHora, string profissionalId)
    {
        var servico = await db.Servicos.Find(s => s.Id == servicoId && s.Status == "ativo").FirstOrDefaultAsync();
        if (servico == null) return NotFound();
        if (dataHora < DateTime.Now.AddHours(24))
        {
            TempData["Erro"] = "O agendamento precisa ser feito com pelo menos 24 horas de antecedência.";
            return RedirectToAction(nameof(Servico), new { id = servicoId });
        }
        var cliente = await db.Clientes.Find(c => c.EmpresaId == servico.EmpresaId && c.Email == email).FirstOrDefaultAsync();
        if (cliente == null)
        {
            cliente = new Cliente { EmpresaId = servico.EmpresaId, Nome = nome, Email = email, Telefone = telefone };
            await db.Clientes.InsertOneAsync(cliente);
        }
        var profissional = await db.Profissionais.Find(p => p.Id == profissionalId && p.EmpresaId == servico.EmpresaId && p.Status == "ativo").FirstOrDefaultAsync();
        if (profissional == null) { TempData["Erro"] = "Escolha um profissional válido."; return RedirectToAction(nameof(Servico), new { id = servicoId }); }
        var conflito = await db.Agendamentos.Find(a => a.EmpresaId == servico.EmpresaId && a.ProfissionalId == profissionalId && a.DataHoraInicio == dataHora && a.Status == "agendado").AnyAsync();
        if (conflito) { TempData["Erro"] = "Esse horário já foi reservado. Escolha outro."; return RedirectToAction(nameof(Servico), new { id = servicoId }); }
        await db.Agendamentos.InsertOneAsync(new Agendamento { EmpresaId = servico.EmpresaId, ServicoId = servico.Id, ClienteId = cliente.Id, ProfissionalId = profissionalId, DataHoraInicio = dataHora });
        TempData["Mensagem"] = "Pedido recebido! A empresa já pode visualizar o seu agendamento.";
        return RedirectToAction(nameof(Servico), new { id = servicoId });
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(string servicoId, string email, DateTime dataHora)
    {
        var servico = await db.Servicos.Find(s => s.Id == servicoId && s.Status == "ativo").FirstOrDefaultAsync();
        if (servico == null) return NotFound();
        if (dataHora < DateTime.Now.AddHours(24)) { TempData["Erro"] = "O cancelamento só pode ser feito com 24 horas de antecedência."; return RedirectToAction(nameof(Servico), new { id = servicoId }); }
        var cliente = await db.Clientes.Find(c => c.EmpresaId == servico.EmpresaId && c.Email == email).FirstOrDefaultAsync();
        var agendamento = cliente == null ? null : await db.Agendamentos.Find(a => a.EmpresaId == servico.EmpresaId && a.ServicoId == servicoId && a.ClienteId == cliente.Id && a.DataHoraInicio == dataHora && a.Status == "agendado").FirstOrDefaultAsync();
        if (agendamento == null) { TempData["Erro"] = "Não encontramos um agendamento com esses dados."; return RedirectToAction(nameof(Servico), new { id = servicoId }); }
        await db.Agendamentos.UpdateOneAsync(a => a.Id == agendamento.Id, Builders<Agendamento>.Update.Set(a => a.Status, "cancelado"));
        TempData["Mensagem"] = "Agendamento cancelado com sucesso."; return RedirectToAction(nameof(Servico), new { id = servicoId });
    }
}
