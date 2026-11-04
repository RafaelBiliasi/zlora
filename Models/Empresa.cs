using MongoDB.Bson.Serialization.Attributes;
namespace Zlora.Models;

public class Empresa
{
    [BsonId] public string Id { get; set; } = Guid.NewGuid().ToString();
    [BsonElement("nome_fantasia")] public string NomeFantasia { get; set; } = "";
    [BsonElement("proprietario_usuario_id")] public string ProprietarioUsuarioId { get; set; } = "";
    [BsonElement("status")] public string Status { get; set; } = "ativa";
    [BsonElement("criado_em")] public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
