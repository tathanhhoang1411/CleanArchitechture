using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Entites.Entites
{
    [Table("Conversations")]
    public class Conversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; } // Nullable, null if 1-1 chat

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastMessageAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Participant> Participants { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
