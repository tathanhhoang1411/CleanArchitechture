using AutoMapper;
using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Entites.Interfaces;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Repository
{
    public class CommentServices : ICommentServices
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _cache;
        private readonly RabbitMQService _rabbitMQ;
        private readonly ILogger<CommentServices> _logger;
        public CommentServices(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheService cache, RabbitMQService rabbitMQ, ILogger<CommentServices> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _rabbitMQ = rabbitMQ ?? throw new ArgumentNullException(nameof(rabbitMQ));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<List<CommentsDto>> GetList_Comment_ByOwner(int skip, int take, string str, long userID, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"comments:user:{userID}:skip:{skip}:take:{take}:q:{str}";
            var cached = await _cache.GetAsync<List<CommentsDto>>(cacheKey);
            if (cached != null)
                return cached;

            // Validate inputs
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;

            List<Comment> comments = null;
            try
            {
                comments = await _unitOfWork.Comments.GetListComment(skip, take, str, userID, cancellationToken);
                await _cache.SetAsync(cacheKey, comments, TimeSpan.FromMinutes(1));
                return _mapper.Map<List<CommentsDto>>(comments ?? new List<Comment>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList_Comment_ByOwner error");
                return new List<CommentsDto>();
            }
        }

        public async Task<List<CommentsDto>> GetList_Comment_ByReviewID(int skip, int take, int reivewID, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"comments:reviewID:{reivewID}:skip:{skip}:take:{take}";
            var cached = await _cache.GetAsync<List<CommentsDto>>(cacheKey);
            if (cached != null)
                return cached;

            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;

            List<Comment> comments = null;
            try
            {
                comments = await _unitOfWork.Comments.GetCommentsByIdReview(skip, take, reivewID, cancellationToken);
                await _cache.SetAsync(cacheKey, comments, TimeSpan.FromMinutes(1));
                return _mapper.Map<List<CommentsDto>>(comments ?? new List<Comment>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList_Comment_ByReviewID error");
                return new List<CommentsDto>();
            }
        }
        public async Task<CommentsDto> Comment_Create(Comment comments, CancellationToken cancellationToken = default)
        {
            if (comments == null) throw new ArgumentNullException(nameof(comments));
            Comment comment = null;
            try
            {
                comment = await _unitOfWork.Comments.CreateAComment(comments, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"CommentCreate:{comment.CommentId}");
                return _mapper.Map<CommentsDto>(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Comment_Create error");
                return null;
            }
        }

    }
}
