using Database;
using Database;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.Auth;
using Services.Auth.Contracts;
using zblog.Models.Auth;
using zblog.Services.Auth.Contracts;

namespace Services.Auth;

public class AuthServices : IAuthServices
{
  private readonly ITokenService _tokenService;
  private readonly UserManager<User> _userManager;
  private readonly ILogger<AuthServices> _logger;
  private readonly IOptions<JwtSettings> _jwtSettings;
  private readonly ApplicationDbContext _context;

  public AuthServices(UserManager<User> userManager, ILogger<AuthServices> logger, ITokenService tokenService, IOptions<JwtSettings> jwtSettings, ApplicationDbContext context)
  {
    _userManager = userManager;
    _logger = logger;
    _tokenService = tokenService;
    _jwtSettings = jwtSettings;
    _context = context;
  }
  public async Task<AuthRegisterResult> RegisterAsync(RegisterRequestDto registerRequestDto)
  {  
    var normalizedUsername = registerRequestDto.Username.Trim().ToLowerInvariant();
    var normalizedEmail = registerRequestDto.Email.Trim().ToLowerInvariant();
    // check if the user is already registered
    // by username
    var existingUser = await _userManager.FindByNameAsync(normalizedUsername);
    if (existingUser is not null)
    {
      return new AuthRegisterResult(
        IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName", Description = "Username is already taken." }),
        null
      );
    }
    // by email
    existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
    if (existingUser is not null)
    {
      return new AuthRegisterResult(
        IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email is already taken." }),
        null
      );
    }
    // Email Confirmation

    // map DTO to user
    var user = new User
    {
      Id = Guid.NewGuid(),
      Name = registerRequestDto.Name,
      UserName = normalizedUsername,
      Email = normalizedEmail,
      PhoneNumber = registerRequestDto.Phone,
      created_at= DateTime.UtcNow,
      updated_at= DateTime.UtcNow
    };
    // Save to DB (Identity handles password hashing automatically)
    var result = await _userManager.CreateAsync(user, registerRequestDto.Password);
    if (!result.Succeeded)
    {
      return new AuthRegisterResult(result, null);
    }

    var roleName = user_type_option.member.ToString();

    // Assign the requested role, defaulting to member.
    var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
    if (!addToRoleResult.Succeeded)
    {
      return new AuthRegisterResult(addToRoleResult, null);
    }

    // Generate JWT token
    var (token, expiresAt) = _tokenService.CreateAccessToken(user, new List<string> { roleName });

    return new AuthRegisterResult(
      IdentityResult.Success,
      new RegisterResponseDto(
        user.Id,
        "User registered successfully.",
        user.UserName!,
        user.Email!,
        new List<string> { roleName },
        token,
        expiresAt
      )
    );
  }

  public async Task<AuthLoginResult> LoginAsync(LoginRequestDto loginRequestDto)
  {
    // validate credentials
    var normalizedUsername = loginRequestDto.Username.Trim().ToLowerInvariant();
    var user = await _userManager.FindByNameAsync(normalizedUsername);
    if (user == null)
    {
      return new AuthLoginResult(
        IdentityResult.Failed(new IdentityError { Code = "InvalidCredentials", Description = "Invalid credentials." }),
        null
      );
    }
    // validate password
    var result = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
    if (!result)
    {
      return new AuthLoginResult(
        IdentityResult.Failed(new IdentityError { Code = "InvalidCredentials", Description = "Invalid credentials." }),
        null
      );
    }
  
    var roles = (await _userManager.GetRolesAsync(user)).ToList();

    // generate JWT token(access)
    var (accessToken, accessExpiresAt) = _tokenService.CreateAccessToken(user, roles);

    // Issue a refresh token and store it so we can rotate or revoke it later.
    var refreshToken = _tokenService.CreateRefreshToken();
    var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.Value.RefreshTokenDays);

    return new AuthLoginResult(
      IdentityResult.Success,
      new LoginResponseDto(
        user.Id,
        "User logged in successfully.",
        user.UserName!,
        user.Email!,
        roles,
        accessToken,
        accessExpiresAt,
        refreshToken,
        refreshExpiresAt
      )
    );
  }
  public Task<AuthLogoutResult> LogoutAsync()
  {
    // The frontend deletes the JWT.
    // The backend deletes or revokes the Refresh Token from the database.
    return Task.FromResult(new AuthLogoutResult(
      IdentityResult.Success,
      new LogoutResponseDto("User logged out successfully.")
    ));
  }

  public async Task<AuthRefreshResult> RefreshAsync(RefreshRequest request)
  {
      /* I do four things: refersh token is a single-use token.
        1. Validate refresh token:
        2. Detect reused token:
          - if revoked(used/canceled) token have been reused again(stolen)
            - then revoke all active tokens.
        3. Rotate refresh token:
          - revoke old token and replace it with a new token.
        4. Issue a new access(jwt) token.
      */
      
      // 1. Validate refresh token
      var existing = await _context.refresh_tokens
          .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

      if (existing is null)
      {
          return new AuthRefreshResult(
              IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Refresh token not found." }),
              null
          );
      }
      if (!existing.IsActive)
      {
          // 2. Detect reused token
          if (existing.Revoked is not null)
          {
              await RevokeAllActiveTokensAsync(existing.UserId);
          }
          return new AuthRefreshResult(
              IdentityResult.Failed(new IdentityError { Code = "TokenExpired", Description = "Refresh token is no longer active." }),
              null
          );
      }

      var user = await _userManager.FindByIdAsync(existing.UserId.ToString());
      if (user is null)
      {
          return new AuthRefreshResult(
              IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "User not found." }),
              null
          );
      }

      var newRefreshToken = _tokenService.CreateRefreshToken();
      var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.Value.RefreshTokenDays);
      
      // 3. Rotate refresh token
      existing.Revoked = DateTime.UtcNow;
      existing.ReplacedByToken = newRefreshToken;

      _context.refresh_tokens.Add(new RefreshToken
      {
          Token = newRefreshToken,
          UserId = user.Id,
          Created = DateTime.UtcNow,
          Expires = refreshExpiresAt
      });
      await _context.SaveChangesAsync();

      var roles = (await _userManager.GetRolesAsync(user)).ToList();
      
      // 4. Issue a new access(jwt) token.
      var (accessToken, accessExpiresAt) = _tokenService.CreateAccessToken(user, roles);

      return new AuthRefreshResult(
          IdentityResult.Success,
          new RefreshResponseDto(
              user.Id,
              user.Email!,
              roles,
              accessToken,
              accessExpiresAt,
              newRefreshToken,
              refreshExpiresAt
          )
      );
  }

  private async Task RevokeAllActiveTokensAsync(Guid userId)
  {
      var activeTokens = await _context.refresh_tokens
          .Where(t => t.UserId == userId && t.Revoked == null && t.Expires > DateTime.UtcNow)
          .ToListAsync();

      foreach (var token in activeTokens)
      {
          token.Revoked = DateTime.UtcNow;
      }

      await _context.SaveChangesAsync();
  }
}
