using CleanArchitecture.Entites.Entites;

namespace CleanArchitecture.Application.Dtos
{
    public class ConversationDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Avatar { get; set; } // Can be group avatar or friend's avatar
        public DateTime LastMessageAt { get; set; }
        public MessageDto? LastMessage { get; set; }
        public List<UserDto> Participants { get; set; }

        public ConversationDto()
        {
            Participants = new List<UserDto>();
        }
    }

    public class UserDto
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
    }
}
