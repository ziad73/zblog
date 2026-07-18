using Entities;

namespace Services.Auth.Contracts;
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user, IEnumerable<string> roles);
    public string CreateRefreshToken();

}
