using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Entites.Entites
{
    public class Comment
    {
        [Key]
        public long CommentId { get; set; }

        public int ReviewId { get; set; }

        public long UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string CommentText { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
