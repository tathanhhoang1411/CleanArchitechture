using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IReviewDto
    {
        public int ReviewId { get; set; }
        public long OwnerID { get; set; }
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        [MaxLength(1000)] // Đặt độ dài tối đa là 100 ký tự
        public DateTime CreatedAt { get; set; }
    }
}
