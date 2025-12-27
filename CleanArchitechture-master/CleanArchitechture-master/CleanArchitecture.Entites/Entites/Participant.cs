using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Entites.Entites
{
    [Table("Participants")]
    public class Participant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Conversation")]
        public long ConversationId { get; set; }

        [ForeignKey("User")]
        public long UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Conversation Conversation { get; set; }
        public virtual User User { get; set; }
    }
}
