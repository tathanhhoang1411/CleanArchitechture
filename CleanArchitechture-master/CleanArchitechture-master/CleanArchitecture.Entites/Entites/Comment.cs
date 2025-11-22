
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Entites.Entites
{
    public class Comment
    {
        [Key]
        public long CommentId { get; set; }
        public int ReviewId { get; set; }
        public long UserId { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
