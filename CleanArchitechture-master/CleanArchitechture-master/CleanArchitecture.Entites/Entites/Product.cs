using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Entites.Entites
{
    public class Product
    {
        [Key]
        public string ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; }

        [MaxLength(2048)]
        public string? ProductImage1 { get; set; }
        [MaxLength(2048)]
        public string? ProductImage2 { get; set; }
        [MaxLength(2048)]
        public string? ProductImage3 { get; set; }
        [MaxLength(2048)]
        public string? ProductImage4 { get; set; }
        [MaxLength(2048)]
        public string? ProductImage5 { get; set; }

        public decimal Price { get; set; }

        public long OwnerID { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public int ReviewID { get; set; }
        public int Type { get; set; }
    }
}
