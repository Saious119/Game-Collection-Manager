using System.Security.Claims;

namespace GameCollectionManagerAPI.Services
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string username, string email);
    }
}