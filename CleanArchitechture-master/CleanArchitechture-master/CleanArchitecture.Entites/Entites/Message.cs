using CleanArchitecture.Entites.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Entites.Entites
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Conversation")]
        public long ConversationId { get; set; }

        [ForeignKey("Sender")]
        public long SenderId { get; set; }

        [Required]
        public string Content { get; set; }

        public MessageType MessageType { get; set; } = MessageType.Text;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Navigation properties
        public virtual Conversation Conversation { get; set; }
        public virtual User Sender { get; set; }
        public virtual CallHistory CallHistory { get; set; }
    }
}
