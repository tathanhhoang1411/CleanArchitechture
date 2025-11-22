
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Entites.Entites
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        public long OwnerID { get; set; }
        public double Rating { get; set; }
        public string ReviewText { get; set; }
        [MaxLength(1000)] // Đặt độ dài tối đa là 100 ký tự
        public DateTime CreatedAt { get; set; }
    }
}
