
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Entites.Entites
{
    public class User
    {

        public long UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        [Key]
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public Boolean Status { get; set; }
    }
}
