
using CleanArchitecture.Entites.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Dtos
{ 
public class ReviewDto : IReviewDto
    {
        public int ReviewId { get; set; }
        public long OwnerID { get; set; }
        public string? ProductId { get; set; }
        public double Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
