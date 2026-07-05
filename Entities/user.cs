using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Entities
{
  public class user
  {
    public Guid id { get; set; }
    [Required]
    public string username { get; set; } // unique
    public string email { get; set; }
    public DateTime created_at { get; set; } = DateTime.UtcNow;
    public DateTime updated_at { get; set; } = DateTime.UtcNow;
  }
  
}
