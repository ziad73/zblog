using Microsoft.AspNetCore.Identity;
using Models.Auth;

namespace Services.Auth.Contracts;

public record AuthLoginResult(
  IdentityResult IdentityResult,
  LoginResponseDto? Response
);
