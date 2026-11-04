using MongoDB.Driver;
using Zlora.Data;
using Zlora.Models;

namespace Zlora.Seeds;

public static class DemoDataSeeder
{
    public static async Task Seed(ContextMongoDb db)
    {
        if (!await db.Planos.Find(_ => true).AnyAsync())
        {
            await db.Planos.InsertManyAsync(new[]
            {
                new Plano { Codigo = "basico", Nome = "Básico", Descricao = "Para começar", PrecoMensal = 49.90m, LimiteProfissionais = 2 },
                new Plano { Codigo = "profissional", Nome = "Profissional", Descricao = "Para equipes em crescimento", PrecoMensal = 99.90m, LimiteProfissionais = 10 },
                new Plano { Codigo = "premium", Nome = "Premium", Descricao = "Para operações completas", PrecoMensal = 199.90m, LimiteProfissionais = 30 }
            });
        }

        // Massa de demonstração solicitada: 50 empresas, 30 clientes e 28 agendamentos.
        if (!await db.Empresas.Find(_ => true).AnyAsync())
        {
            var empresas = Enumerable.Range(1, 50).Select(i => new Empresa { NomeFantasia = $"Espaço Zlora {i:00}", ProprietarioUsuarioId = "demo", Status = "ativa" }).ToList();
            await db.Empresas.InsertManyAsync(empresas);
            var empresaId = empresas[0].Id;
            var profissionais = new[] { "Ana Souza", "Beatriz Lima", "Carla Mendes", "Daniela Reis" }
                .Select((nome, i) => new Profissional { EmpresaId = empresaId, NomeExibicao = nome, Horario = i % 2 == 0 ? "08:00 - 17:00" : "10:00 - 19:00" }).ToList();
            await db.Profissionais.InsertManyAsync(profissionais);
            var servicos = new[] { ("Corte e escova", 75m), ("Manicure", 45m), ("Massagem relaxante", 120m), ("Design de sobrancelhas", 55m) }
                .Select((s, i) => new Servico { EmpresaId = empresaId, Nome = s.Item1, Descricao = "Serviço de demonstração", Preco = s.Item2, ProfissionalId = profissionais[i % profissionais.Count].Id }).ToList();
            await db.Servicos.InsertManyAsync(servicos);
            var clientes = Enumerable.Range(1, 30).Select(i => new Cliente { EmpresaId = empresaId, Nome = $"Cliente Demonstração {i:00}", Telefone = $"(14) 99999-{i:0000}", Email = $"cliente{i:00}@exemplo.com" }).ToList();
            await db.Clientes.InsertManyAsync(clientes);
            var agora = DateTime.Now;
            var agendamentos = Enumerable.Range(0, 28).Select(i => new Agendamento
            {
                EmpresaId = empresaId, ClienteId = clientes[i % clientes.Count].Id, ProfissionalId = profissionais[i % profissionais.Count].Id,
                ServicoId = servicos[i % servicos.Count].Id, DataHoraInicio = agora.Date.AddDays(i - 14).AddHours(9 + (i % 7)), Status = i < 14 ? "concluido" : "agendado"
            }).ToList();
            await db.Agendamentos.InsertManyAsync(agendamentos);
        }
    }
}
