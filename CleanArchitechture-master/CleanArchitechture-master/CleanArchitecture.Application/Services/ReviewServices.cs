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

        public async Task<ReviewDto> Review_Create(Reviews review)
        {
            ReviewDto reviewDto = null;
            try
            {
                await _unitOfWork.Reviews.CreateReview(review);
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<ReviewDto>(review);
            }
            catch
            {
                return reviewDto;
            }
        }

    }
}
