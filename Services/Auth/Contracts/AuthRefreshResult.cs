using Microsoft.AspNetCore.Identity;
using Models.Auth;

namespace Services.Auth.Contracts;

public record AuthRefreshResult(
  IdentityResult IdentityResult,
  RefreshResponseDto? Response
);
