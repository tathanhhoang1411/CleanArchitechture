using CleanArchitecture.Entites.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Entites
{
    [Table("[Friends]")]
    public class Friend
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RequestId { get; set; } // Khóa chính (PK), bigint

        [Required]
        // Khóa ngoại (Foreign Key) cho người gửi
        public long SenderId { get; set; } // bigint, not null

        [Required]
        // Khóa ngoại (Foreign Key) cho người nhận
        public long ReceiverId { get; set; } // bigint, not null

        [Required]
        // Lưu trữ dưới dạng INT trong DB, nhưng dùng enum trong Code
        public FriendRequestStatus Status { get; set; } // int, not null

        [Required]
        public DateTime RequestedAt { get; set; } // datetime, not null

        public DateTime? ActionedAt { get; set; } // datetime, null (có thể trống)

        // -------------------------------------------------------------
        // Thuộc tính điều hướng (Navigation Properties)
        // -------------------------------------------------------------

        //[ForeignKey(nameof(SenderId))]
        //public User? Sender { get; set; }

        //[ForeignKey(nameof(ReceiverId))]
        //public User? Receiver { get; set; }
    }
}
