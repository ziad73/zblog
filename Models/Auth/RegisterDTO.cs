using System.ComponentModel.DataAnnotations;
using Entities;
namespace Models.Auth;

public record RegisterDTO(
    [Required]
    string Name,
    [Required] 
    [StringLength(20, MinimumLength = 3)] 
    // uniquness
    string Username,

    [Required] 
    [EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
    string Email,

    [DataType(DataType.PhoneNumber)]
    string Phone,

    [Required] 
    [MinLength(6)] 
    [DataType(DataType.Password)]
    string Password,

    [Required] 
    [MinLength(6)] 
    [DataType(DataType.Password)]
    [property: Compare("Password", ErrorMessage = "Password and Confirm Password must match")]
    string ConfirmPassword,

    user_type_option UserType = user_type_option.User

)
{
    public User ToUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = Name,
            UserName = Username,
            Email = Email,
            PhoneNumber = Phone,
            created_at= DateTime.UtcNow,
            updated_at= DateTime.UtcNow
        };
    }
}
