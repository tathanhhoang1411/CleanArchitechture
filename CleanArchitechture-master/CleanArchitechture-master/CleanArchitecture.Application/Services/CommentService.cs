using AutoMapper;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Repository
{
    public class CommentServices : ICommentServices
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public CommentServices(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<List<CommentsDto>> GetList_Comment_ByOwner(int skip, int take, string str, long userID)
        {
            List<Comments> comments=null;
            try
            {
                comments = await _unitOfWork.Comments.GetListComment(skip,take, str, userID);
                return _mapper.Map<List<CommentsDto>>(comments);
            }
            catch
            {
                return null;
            }
        }
        public async Task<CommentsDto> Comment_Create(Comments comments)
        {
            Comments comment=null;
            try
            {
                comment = await _unitOfWork.Comments.CreateAComment(comments);
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<CommentsDto>(comment);
            }
            catch
            {
                return null;
            }
        }

    }
}
