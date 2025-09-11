using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.IRepository
{
    public interface ICommentServices
    {

        Task<List<CommentsDto>> GetList_Comment_ByOwner(int skip, int take, string str, long userID);
        Task<CommentsDto> Comment_Create(Comments comments);
    }
}
