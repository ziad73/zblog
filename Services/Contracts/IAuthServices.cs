using Entities;
using Microsoft.AspNetCore.Identity;
using Models.Auth;

namespace Services.Contracts;

public interface IAuthServices
{
  Task<AuthRegisterResult> RegisterAsync(RegisterRequestDto registerRequestDto);
  // Task<User> Login(LoginDTO loginDTO);
  Task<bool> Logout();
}
