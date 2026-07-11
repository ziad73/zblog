using Microsoft.AspNetCore.Identity;
using Models.Auth;

namespace Services.Auth.Contracts;

public record AuthLogoutResult(
  IdentityResult IdentityResult,
  LogoutResponseDto? Response
);
