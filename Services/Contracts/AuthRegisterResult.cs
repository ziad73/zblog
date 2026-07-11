using Microsoft.AspNetCore.Identity;
using Models.Auth;

namespace Services.Contracts;

public record AuthRegisterResult(
  IdentityResult IdentityResult,
  RegisterResponseDto? Response
);
