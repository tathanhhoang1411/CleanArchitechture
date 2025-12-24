using System;

namespace CleanArchitecture.Application.Dtos
{
    public class UserWithDetailDto
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string? Avatar { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // UserDetail
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }

        // Count of accepted friends
        public int FriendCount { get; set; }
    }
}