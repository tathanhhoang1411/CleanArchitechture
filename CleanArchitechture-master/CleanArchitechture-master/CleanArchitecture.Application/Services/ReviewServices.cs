using AutoMapper;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Repository
{
    public class ReviewServices : IReviewServices
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _cache;
        private readonly RabbitMQService _rabbitMQ;
        private readonly ILogger<ReviewServices> _logger;
        private const int _maxTake = 5000;
        public ReviewServices(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheService cache, RabbitMQService rabbitMQ, ILogger<ReviewServices> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _rabbitMQ = rabbitMQ ?? throw new ArgumentNullException(nameof(rabbitMQ));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetList_Reviews(int skip, int take, string str, long userID, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"reviews:user:{userID}:skip:{skip}:take:{take}:q:{str}";
            var cached = await _cache.GetAsync<List<object>>(cacheKey);
            if (cached != null)
                return cached;
            List<object> reviews = null;
            try
            {
                if (skip < 0) skip = 0;
                if (take <= 0) take = 10;
                take = Math.Min(take, _maxTake);

                reviews = await _unitOfWork.Reviews.GetListReviews(skip, take, str, userID, cancellationToken);
                await _cache.SetAsync(cacheKey, reviews, TimeSpan.FromMinutes(1));
                return reviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList_Reviews error");
                return new List<object>();
            }
        }
        //Hàm để kiểm tra bài review đó có phải của tài khoản 
        public async Task<List<Review>> GetList_Reviews_ByOwner(int reviewID, long ownerID, CancellationToken cancellationToken = default)
        {
            try
            {
                var aReview = await _unitOfWork.Reviews.GetListReviewsByOwnerID(reviewID, ownerID, cancellationToken);
                return aReview ?? new List<Review>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList_Reviews_ByOwner error");
                return new List<Review>();
            }
        }

        public async Task<ReviewsDto> Review_Create(Review review, CancellationToken cancellationToken = default)
        {
            if (review == null) return null;
            try
            {
                if (review.CreatedAt == default)
                    review.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Reviews.CreateReview(review, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"ReviewCreate:{review.ReviewId}");
                return _mapper.Map<ReviewsDto>(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Review_Create error");
                return null;
            }
        }
        public async Task<int> Review_Del(int reviewID, long ownerID, CancellationToken cancellationToken = default)
        {
            int reviewIDDel = -1;
            try
            {
                List<Review> list = await GetList_Reviews_ByOwner(reviewID, ownerID, cancellationToken);
                if (list == null || list.Count == 0)
                {
                    return reviewIDDel;
                }
                await _unitOfWork.Products.DelListProduct(reviewID, cancellationToken);
                await _unitOfWork.Comments.DelListComment(reviewID, cancellationToken);
                reviewIDDel = await _unitOfWork.Reviews.DelReview(reviewID, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                _rabbitMQ.Publish($"ReviewDelete:{reviewID}");
                return reviewIDDel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Review_Del error");
                return reviewIDDel;
            }
        }

    }
}
