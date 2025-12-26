using System;
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Application.Dtos
{
    public class UserWithDetailDto
    {
        public long UserId { get; set; }
        public string? Username { get; set; }
        public string? Avatar { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // UserDetail
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
        public string? FirstName { get; set; }
        public string? Phone { get; set; }
        public string? LastName { get; set; }
        public int Gender { get; set; }
        public int CountryCode { get; set; } // Mặc định là nvarchar(max)
        public int Material { get; set; } // Mặc định là nvarchar(max)

        // Count of accepted friends
        public int FriendCount { get; set; }
    }
}