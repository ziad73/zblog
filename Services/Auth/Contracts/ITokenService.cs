using Entities;

namespace zblog.Services.Auth.Contracts;
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user, IEnumerable<string> roles);
}
