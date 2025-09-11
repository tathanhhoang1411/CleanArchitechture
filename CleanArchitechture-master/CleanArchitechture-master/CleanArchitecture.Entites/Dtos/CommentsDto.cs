
using CleanArchitecture.Entites.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CleanArchitecture.Entites.Dtos
{
    public class CommentsDto: ICommentDto
    {
        public int CommentId { get; set; }
        public int ReviewId { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
