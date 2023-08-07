using Blog.Models;
using System.Security.Claims;

namespace Blog.Extensions;

public static class RoleClaimsExtension
{
    // Pegar os Roles do usuario e transformar numa List de Claim, quando usar o User.GetClaims
    public static IEnumerable<Claim> GetClaims(this User user)
    {
        var result = new List<Claim>()
        {
            new(ClaimTypes.Name, user.Email) // Isso aqui vira o User.Identity.Name
        };
        // Adicinar os roles do usuario, pegar todos os roles que estão no usuario e retornar deles um Claim
        result.AddRange(
            user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Slug))
        );
        return result;
    }
}
