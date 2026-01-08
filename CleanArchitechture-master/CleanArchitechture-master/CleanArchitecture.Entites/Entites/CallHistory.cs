using CleanArchitecture.Entites.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Entites.Entites
{
    [Table("CallHistory")]
    public class CallHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CallId { get; set; }

        [Required]
        [ForeignKey("Caller")]
        public long CallerId { get; set; }

        [Required]
        [ForeignKey("Receiver")]
        public long ReceiverId { get; set; }

        [Required]
        public CallType CallType { get; set; }

        [Required]
        public CallStatus Status { get; set; } = CallStatus.Missed;

        [Required]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// Thời gian cuộc gọi (tính bằng giây)
        /// </summary>
        public int? Duration { get; set; }

        [ForeignKey("Message")]
        public long? MessageId { get; set; }

        // Navigation properties
        public virtual User Caller { get; set; }
        public virtual User Receiver { get; set; }
        public virtual Message Message { get; set; }
    }
}
