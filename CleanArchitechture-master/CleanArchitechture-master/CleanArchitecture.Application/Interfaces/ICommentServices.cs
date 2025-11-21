using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface ICommentServices
    {

        Task<List<CommentsDto>> GetList_Comment_ByOwner(int skip, int take, string str, long userID);
        Task<List<CommentsDto>> GetList_Comment_ByReviewID(int skip, int take, int reviewID);
        Task<CommentsDto> Comment_Create(Comment comments);
    }
}
