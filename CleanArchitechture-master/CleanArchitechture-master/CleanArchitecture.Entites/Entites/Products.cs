using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Entites
{
    public class Products
    {
        [Key]
        public string ProductId { get; set; }
        [MaxLength(100)] // Đặt độ dài tối đa là 100 ký tựs
        public string ProductName { get; set; }
        public string? ProductImage1 { get; set; }
        public string? ProductImage2 { get; set; }
        public string? ProductImage3 { get; set; }
        public string? ProductImage4 { get; set; }
        public string? ProductImage5 { get; set; }
        public decimal Price { get; set; }
        public long OwnerID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
