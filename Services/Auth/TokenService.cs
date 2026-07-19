using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Models.Auth;
using Services.Auth.Contracts;

namespace Services.Auth;
public class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public (string Token, DateTime ExpiresAt) CreateAccessToken(User user, IEnumerable<string> roles, IEnumerable<Claim>? additionalClaims = null)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);// +

        // Claims are the pieces of information we store inside the payload, key-value string pairs.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),// Subject: holds the user ID
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Name, $"{user.Name}"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())// JWT ID
        };

        // One "role" claim per role the user has. {"role": "Admin", "role": "Editor"}
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        if (additionalClaims is not null)
            claims.AddRange(additionalClaims);// Add custom claims into token

        // Key as plain-text into a byte array
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);// hash algorithm
        
        var descriptor = new SecurityTokenDescriptor
        {
            // payload
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            // signature
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);

        return (token, expiresAt);
    }

    public string CreateRefreshToken()
    {
        // A refresh token is just a large cryptographically-random value.
        // RandomNumberGenerator replaces the obsolete RNGCryptoServiceProvider.
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
