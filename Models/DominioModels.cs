using MongoDB.Bson.Serialization.Attributes;

namespace Zlora.Models;

public class Plano
{
    [BsonId] public string Id { get; set; } = Guid.NewGuid().ToString();
    [BsonElement("codigo")] public string Codigo { get; set; } = "basico";
    [BsonElement("nome")] public string Nome { get; set; } = "Básico";
    [BsonElement("descricao")] public string Descricao { get; set; } = "Plano inicial";
    [BsonElement("preco_mensal")] public decimal PrecoMensal { get; set; }
    [BsonElement("limite_profissionais")] public int LimiteProfissionais { get; set; }
}

public class Cliente
{
    [BsonId] public string Id { get; set; } = Guid.NewGuid().ToString();
    [BsonElement("empresa_id")] public string EmpresaId { get; set; } = "";
    [BsonElement("nome")] public string Nome { get; set; } = "";
    [BsonElement("telefone")] public string Telefone { get; set; } = "";
    [BsonElement("email")] public string Email { get; set; } = "";
    [BsonElement("observacoes")] public string Observacoes { get; set; } = "";
    [BsonElement("criado_em")] public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

public class Profissional
{
    [BsonId] public string Id { get; set; } = Guid.NewGuid().ToString();
    [BsonElement("empresa_id")] public string EmpresaId { get; set; } = "";
    [BsonElement("usuario_id")] public string UsuarioId { get; set; } = "";
    [BsonElement("nome_exibicao")] public string NomeExibicao { get; set; } = "";
    [BsonElement("horario")] public string Horario { get; set; } = "08:00 - 18:00";
    [BsonElement("status")] public string Status { get; set; } = "ativo";
}

public class Servico
{
    [BsonId] public string Id { get; set; } = Guid.NewGuid().ToString();
    [BsonElement("empresa_id")] public string EmpresaId { get; set; } = "";
    [BsonElement("nome")] public string Nome { get; set; } = "";
    [BsonElement("descricao")] public string Descricao { get; set; } = "";
    [BsonElement("duracao_minutos")] public int DuracaoMinutos { get; set; } = 60;
    [BsonElement("preco")] public decimal Preco { get; set; }
    [BsonElement("profissional_id")] public string ProfissionalId { get; set; } = "";
    [BsonElement("status")] public string Status { get; set; } = "ativo";
}

public class Agendamento
{
    [BsonId] public string Id { get; set; } = Guid.NewGuid().ToString();
    [BsonElement("empresa_id")] public string EmpresaId { get; set; } = "";
    [BsonElement("cliente_id")] public string ClienteId { get; set; } = "";
    [BsonElement("profissional_id")] public string ProfissionalId { get; set; } = "";
    [BsonElement("servico_id")] public string ServicoId { get; set; } = "";
    [BsonElement("data_hora_inicio")] public DateTime DataHoraInicio { get; set; }
    [BsonElement("status")] public string Status { get; set; } = "agendado";
    [BsonElement("observacoes")] public string Observacoes { get; set; } = "";
}

public class Assinatura
{
    [BsonId] public string Id { get; set; } = Guid.NewGuid().ToString();
    [BsonElement("empresa_id")] public string EmpresaId { get; set; } = "";
    [BsonElement("plano_id")] public string PlanoId { get; set; } = "";
    [BsonElement("status")] public string Status { get; set; } = "ativa";
    [BsonElement("valor_contratado")] public decimal ValorContratado { get; set; }
}
