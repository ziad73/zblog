using Entities;
using Microsoft.AspNetCore.Identity;
using Models.Auth;

namespace Services.Auth.Contracts;

public interface IAuthServices
{
  Task<AuthRegisterResult> RegisterAsync(RegisterRequestDto registerRequestDto);
  // Login
  Task<AuthLoginResult> LoginAsync(LoginRequestDto loginRequestDto);
  Task<AuthLogoutResult> LogoutAsync(RevokeRequest request);
  Task<AuthRefreshResult> RefreshAsync(RefreshRequest request);
  Task<AuthLogoutResult> RevokeAsync(RevokeRequest request);
}
