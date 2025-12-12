using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Entites.Entites
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReviewId { get; set; }

        public long OwnerID { get; set; }

        public double Rating { get; set; }

        [Required]
        [MaxLength(2000)]
        public string ReviewText { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
