using MongoDB.Driver;
using Zlora.Models;
namespace Zlora.Data;

public class ContextMongoDb
{
    public IMongoCollection<ApplicationUser> Usuarios { get; }
    public IMongoCollection<Empresa> Empresas { get; }
    public IMongoCollection<Cliente> Clientes { get; }
    public IMongoCollection<Profissional> Profissionais { get; }
    public IMongoCollection<Servico> Servicos { get; }
    public IMongoCollection<Agendamento> Agendamentos { get; }
    public IMongoCollection<Plano> Planos { get; }
    public IMongoCollection<Assinatura> Assinaturas { get; }
    public ContextMongoDb(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoConnection:ConnectionString"]);
        Usuarios = client.GetDatabase(configuration["MongoConnection:Database"]).GetCollection<ApplicationUser>("usuarios");
        Empresas = client.GetDatabase(configuration["MongoConnection:Database"]).GetCollection<Empresa>("empresas");
        var db = client.GetDatabase(configuration["MongoConnection:Database"]);
        Clientes = db.GetCollection<Cliente>("clientes");
        Profissionais = db.GetCollection<Profissional>("profissionais");
        Servicos = db.GetCollection<Servico>("servicos");
        Agendamentos = db.GetCollection<Agendamento>("agendamentos");
        Planos = db.GetCollection<Plano>("planos");
        Assinaturas = db.GetCollection<Assinatura>("assinaturas");
    }
    public async Task CriarIndices()
    {
        await Usuarios.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(
            Builders<ApplicationUser>.IndexKeys.Ascending(u => u.NormalizedEmail), new CreateIndexOptions { Unique = true }));
        await Usuarios.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(Builders<ApplicationUser>.IndexKeys.Ascending(u => u.EmpresaId)));
    }
}
