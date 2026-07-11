using Entities;
using Microsoft.AspNetCore.Identity;

namespace Database;

public static class IdentityRoleSeeder
{
  private static readonly string[] DefaultRoles =
  [
    user_type_option.member.ToString(),
    user_type_option.author.ToString(),
    user_type_option.admin.ToString()
  ];

  public static async Task SeedAsync(IServiceProvider serviceProvider)
  {
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<user_role>>();

    foreach (var roleName in DefaultRoles)
    {
      if (await roleManager.RoleExistsAsync(roleName))
      {
        continue;
      }

      await roleManager.CreateAsync(new user_role { Name = roleName });
    }
  }
}
