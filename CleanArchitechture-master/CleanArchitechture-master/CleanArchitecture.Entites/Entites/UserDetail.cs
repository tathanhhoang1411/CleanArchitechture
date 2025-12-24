using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Entites
{
    [Table("UserDetails", Schema = "dbo")]
    public class UserDetail
    {
        [Key]
        [ForeignKey("User")]
        public long UserId { get; set; } // Vừa là PK vừa là FK

        [Column(TypeName = "date")]
        public DateTime? BirthDate { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public string Bio { get; set; } // Mặc định là nvarchar(max)

        // Navigation property (Mối quan hệ 1-1)
        public virtual User User { get; set; }
    }
}
