using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IProductDto
    {
        public int ProductId { get; set; } // Nếu cần thiết, có thể bỏ qua trường này
        public string ProductName { get; set; }
        public string? ProductImage1 { get; set; }
        public string? ProductImage2 { get; set; }
        public string? ProductImage3 { get; set; }
        public string? ProductImage4 { get; set; }
        public string? ProductImage5 { get; set; }
        public decimal Price { get; set; }
    }
}
