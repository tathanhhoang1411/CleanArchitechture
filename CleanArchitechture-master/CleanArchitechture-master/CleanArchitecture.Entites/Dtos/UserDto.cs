using CleanArchitecture.Entites.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Dtos
{
    public class UserDto: IUserDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(5, ErrorMessage = "Username must be at least 5 and at most 100 characters long.", MinimumLength = 5)]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be at least 5 and at most 100 characters long.", MinimumLength = 5)]
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
