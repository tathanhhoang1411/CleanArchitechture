using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Entites
{
    public class Users
    {

        public long UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        [Key]
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
    }
}
