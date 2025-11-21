using AutoMapper;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;

namespace CleanArchitecture.Application.Repository
{
    public class ReviewServices : IReviewServices
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _cache;
        private readonly RabbitMQService _rabbitMQ;
        public ReviewServices(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheService cache, RabbitMQService rabbitMQ)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _rabbitMQ = rabbitMQ ?? throw new ArgumentNullException(nameof(rabbitMQ));
        }

        public async Task<List<object>> GetList_Reviews(int skip, int take, string str, long userID)
        {
            var cacheKey = $"reviews:user:{userID}:skip:{skip}:take:{take}:q:{str}";
            var cached = await _cache.GetAsync<List<object>>(cacheKey);
            if (cached != null)
                return cached;
            List<object> reviews=null;
            try
            {
                reviews = await _unitOfWork.Reviews.GetListReviews(skip,take, str, userID);
                //
                await _cache.SetAsync(cacheKey, reviews, TimeSpan.FromMinutes(1));
                return reviews;
            }
            catch
            {
                return reviews;
            }
        }
        //Hàm để kiểm tra bài review đó có phải của tài khoản 
        public async Task<List<Review>> GetList_Reviews_ByOwner(int reviewID, long ownerID)
        {
            List<Review> aReview = null;
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

        public async Task<ReviewsDto> Review_Create(Review review)
        {
            ReviewsDto reviewDto = null;
            try
            {
                await _unitOfWork.Reviews.CreateReview(review);
                await _unitOfWork.CompleteAsync();
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"ReviewCreate:{review.ReviewId}");
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
                List<Review> list=await GetList_Reviews_ByOwner(reviewID, ownerID);
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
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"ReviewDelete:{reviewID}");
                return reviewIDDel;
            }
            catch
            {
                return reviewIDDel;
            }
        }

    }
}
