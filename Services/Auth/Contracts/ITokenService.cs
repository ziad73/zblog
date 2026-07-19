using System.Security.Claims;
using Entities;

namespace Services.Auth.Contracts;
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user, IEnumerable<string> roles, IEnumerable<Claim>? additionalClaims = null);
    public string CreateRefreshToken();

}
