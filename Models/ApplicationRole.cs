using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
namespace Zlora.Models;

[CollectionName("perfis")]
public class ApplicationRole : MongoDbIdentityRole { }
