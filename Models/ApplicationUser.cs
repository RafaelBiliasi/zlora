using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Zlora.Models;

// Os campos vêm de usuarios e empresas do Banco.txt; Identity cuida da senha.
[CollectionName("usuarios")]
public class ApplicationUser : MongoDbIdentityUser
{
    [BsonElement("empresa_id")] public string EmpresaId { get; set; } = "";
    [BsonElement("recuperacao_hash")] public string? RecuperacaoHash { get; set; }
    [BsonElement("recuperacao_enviada_em")] public DateTime? RecuperacaoEnviadaEm { get; set; }
    [BsonElement("recuperacao_expira_em")] public DateTime? RecuperacaoExpiraEm { get; set; }
    [BsonElement("nome"), MaxLength(120)] public string Nome { get; set; } = "";
    [BsonElement("estabelecimento"), MaxLength(150)] public string Estabelecimento { get; set; } = "";
    [BsonElement("status"), MaxLength(20)] public string Status { get; set; } = "ativo";
    [BsonElement("criado_em")] public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    [BsonElement("alterado_em")] public DateTime AlteradoEm { get; set; } = DateTime.UtcNow;
    [BsonElement("ultimo_acesso")] public DateTime? UltimoAcesso { get; set; }
}
