using Entities;
using Microsoft.AspNetCore.Identity;
using Models.Auth;
using Services.Auth.Contracts;

namespace Services.Auth;

public class AuthServices : IAuthServices
{
  private readonly UserManager<User> _userManager;
  private readonly SignInManager<User> _signInManager;
  private readonly ILogger<AuthServices> _logger;

  public AuthServices(UserManager<User> userManager, ILogger<AuthServices> logger, SignInManager<User> signInManager)
  {
    _userManager = userManager;
    _logger = logger;
    _signInManager = signInManager;
  }
  public async Task<AuthRegisterResult> RegisterAsync(RegisterRequestDto registerRequestDto)
  {  
    var normalizedUsername = registerRequestDto.Username.Trim().ToLowerInvariant();
    var normalizedEmail = registerRequestDto.Email.Trim().ToLowerInvariant();
    // check if the user is already registered
    // by username
    var existingUser = await _userManager.FindByNameAsync(normalizedUsername);
    if (existingUser != null)
    {
      return new AuthRegisterResult(
        IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName", Description = "Username is already taken." }),
        null
      );
    }
    // by email
    existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
    if (existingUser != null)
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

    // Generate Tokens & Respond
    // sign in
    await _signInManager.SignInAsync(user, isPersistent: false);

    return new AuthRegisterResult(
      IdentityResult.Success,
      new RegisterResponseDto(
        user.Id,
        "User registered successfully.",
        user.UserName!,
        user.Email!,
        roleName
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
    // sign in
    await _signInManager.SignInAsync(user, isPersistent: false);

    // jwt token generation from token service

    // return auth cerdentials
    return new AuthLoginResult(
      IdentityResult.Success,
      new LoginResponseDto(
        user.Id,
        "User logged in successfully.",
        user.UserName!,
        user.Email!,
        (await _userManager.GetRolesAsync(user)).ToList()
      )
    );
  }
  public async Task<AuthLogoutResult> LogoutAsync()
  {
    await _signInManager.SignOutAsync();

    return new AuthLogoutResult(
      IdentityResult.Success,
      new LogoutResponseDto("User logged out successfully.")
    );
  }

}
