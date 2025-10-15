using AutoMapper;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.IServices;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using Nest;
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
        private readonly IRedisCacheService _cache;
        private readonly RabbitMQService _rabbitMQ;
        public CommentServices(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheService cache, RabbitMQService rabbitMQ)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _rabbitMQ = rabbitMQ ?? throw new ArgumentNullException(nameof(rabbitMQ));
            // Đăng ký hàm xử lý khi có message mới từ RabbitMQ
            _rabbitMQ.Subscribe(OnMessageReceived);
        }

        public async Task<List<CommentsDto>> GetList_Comment_ByOwner(int skip, int take, string str, long userID)
        {
            var cacheKey = $"comments:user:{userID}:skip:{skip}:take:{take}:q:{str}";
            var cached = await _cache.GetAsync<List<CommentsDto>>(cacheKey);
            if (cached != null)
                return cached;
            List<Comments> comments=null;
            try
            {
                comments = await _unitOfWork.Comments.GetListComment(skip,take, str, userID);
                await _cache.SetAsync(cacheKey, comments, TimeSpan.FromMinutes(1));
                return _mapper.Map<List<CommentsDto>>(comments);
            }
            catch
            {
                return null;
            }
        }
        private void OnMessageReceived(string message)
        {
            Console.WriteLine($"[RabbitMQ] Received message: {message}");

            // Nếu message là thông báo cập nhật comment → xóa cache
            if (message.StartsWith("CommentUpdated:")
                || message.StartsWith("CommentCreate:")
                || message.StartsWith("CommentDelete:"))
            {
                //tự động xóa hết các key cache bắt đầu bằng comments:
                _cache.ClearCacheByPrefix("comments:");
                Console.WriteLine("[RabbitMQ] Cleared cache for comments_ prefix");
            }
        }
        public async Task<List<CommentsDto>> GetList_Comment_ByReviewID(int skip, int take, int reivewID)
        {
            List<Comments> comments=null;
            try
            {
                comments = await _unitOfWork.Comments.GetCommentsByIdReview(skip,take, reivewID);
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
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"CommentCreate:{comment.CommentId}");
                return _mapper.Map<CommentsDto>(comment);
            }
            catch
            {
                return null;
            }
        }

    }
}
