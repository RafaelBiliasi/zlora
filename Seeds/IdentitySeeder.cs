using Microsoft.AspNetCore.Identity;
using Zlora.Models;

namespace Zlora.Seeds;

// Mesmo padrão de criação inicial de perfis de VasosInteligentes.
public static class IdentitySeeder
{
    public static async Task SeedRoles(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var perfil in Perfis.Todos)
            if (!await roles.RoleExistsAsync(perfil)) Check(await roles.CreateAsync(new ApplicationRole { Name = perfil }));

    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
