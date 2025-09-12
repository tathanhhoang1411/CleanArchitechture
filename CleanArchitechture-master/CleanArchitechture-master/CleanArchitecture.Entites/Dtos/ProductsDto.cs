
using CleanArchitecture.Entites.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Dtos
{ 
public class ProductsDto: IProductDto
    {
    public string ProductId { get; set; } // Nếu cần thiết, có thể bỏ qua trường này
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string ProductImage1 { get; set; }
    public string ProductImage2 { get; set; }
    public string ProductImage3 { get; set; }
    public string ProductImage4 { get; set; }
    public string ProductImage5 { get; set; }
    public int ReviewId { get; set; }
    public int Type { get; set; }
}
}
