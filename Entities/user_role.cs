using Microsoft.AspNetCore.Identity;

namespace Entities;
public class user_role:IdentityRole<Guid>
{
  // Id, Name, NormalizedName, ConcurrencyStamp
}
