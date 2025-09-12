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
    public class ReviewServices : IReviewServices
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public ReviewServices(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<List<object>> GetList_Reviews(int skip, int take, string str, long userID)
        {
            List<object> reviews=null;
            try
            {
                reviews = await _unitOfWork.Reviews.GetListReviews(skip,take, str, userID);
                return reviews;
            }
            catch
            {
                return reviews;
            }
        }
        public async Task<List<Reviews>> GetList_Reviews_ByOwner(int reviewID, long ownerID)
        {
            List<Reviews> aReview = null;
            try
            {
                aReview = await _unitOfWork.Reviews.GetListReviewsByOwnerID(reviewID, ownerID);
                return aReview;
            }
            catch
            {
                return aReview;
            }
        }

        public async Task<ReviewsDto> Review_Create(Reviews review)
        {
            ReviewsDto reviewDto = null;
            try
            {
                await _unitOfWork.Reviews.CreateReview(review);
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<ReviewsDto>(review);
            }
            catch
            {
                return reviewDto;
            }
        }
        public async Task<int> Review_Del(int reviewID, long ownerID)
        {
            int reviewIDDel = -1;
            try
            {
                //Check xem bài review cần xóa đó có phải là bài review của tài khoản này không
                List<Reviews> list=await GetList_Reviews_ByOwner(reviewID, ownerID);
                if(list.Count == 0)
                {
                    return reviewIDDel;
                }
                //Xóa product
                await _unitOfWork.Products.DelListProduct(reviewID);
                //Xóa comment
                await _unitOfWork.Comments.DelListComment(reviewID);
                //Xóa review
                reviewIDDel=await _unitOfWork.Reviews.DelReview(reviewID);
                await _unitOfWork.CompleteAsync();
                return reviewIDDel;
            }
            catch
            {
                return reviewIDDel;
            }
        }

    }
}
