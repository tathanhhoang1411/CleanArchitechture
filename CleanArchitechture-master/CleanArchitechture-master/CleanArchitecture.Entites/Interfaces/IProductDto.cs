using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IProductDto
    {
        public string ProductId { get; set; } // Nếu cần thiết, có thể bỏ qua trường này
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }
}
