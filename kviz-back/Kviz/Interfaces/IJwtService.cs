using Kviz.Models;
using System.Security.Claims;

namespace Kviz.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
