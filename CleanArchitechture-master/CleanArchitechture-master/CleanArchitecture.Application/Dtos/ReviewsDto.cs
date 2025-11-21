
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Dtos
{ 
public class ReviewsDto 
    {
        public int ReviewId { get; set; }
        public long OwnerID { get; set; }
        public double Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
