using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationContext _userContext;
        private readonly int _maxTake; // Giới hạn configurable

        public CommentRepository(ApplicationContext userContext, IConfiguration configuration)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // Đọc từ cấu hình, fallback về 1000 nếu không có hoặc không hợp lệ
            var configured = configuration.GetValue<int?>("Paging:MaxTake");
            _maxTake = (configured.HasValue && configured.Value > 0) ? configured.Value : 1000;
        }

        public async Task<List<Comment>> DelListComment(int reviewId, CancellationToken cancellationToken = default)
        {
            // Validate input
            if (reviewId <= 0)
                return new List<Comment>();

            // Lấy danh sách các comment có ReviewID tương ứng
            var commentsToDelete = await _userContext.Comments
                .Where(p => p.ReviewId == reviewId)
                .ToListAsync(cancellationToken);

            if (commentsToDelete == null || commentsToDelete.Count == 0)
            {
                return new List<Comment>();
            }

            _userContext.Comments.RemoveRange(commentsToDelete);

            return commentsToDelete;
        }

        public async Task<List<Comment>> GetListComment(int skip, int take, string str, long userID, CancellationToken cancellationToken = default)
        {
                // Validate pagination
                if (skip < 0) skip = 0;
                if (take <= 0) take = 10;
                take = Math.Min(take, _maxTake);

                var listComment = await _userContext.Comments
                    .Where(p => p.CommentText != null && (p.CommentText.StartsWith(str)) && p.UserId == userID)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                      .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return listComment ?? new List<Comment>();
        }

        public async Task<List<Comment>> GetCommentsByIdReview(int skip, int take, int reviewID, CancellationToken cancellationToken = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            take = Math.Min(take, _maxTake);

            var listComment = await _userContext.Comments
                .Where(p => p.ReviewId == reviewID)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return listComment ?? new List<Comment>();
        }

        public async Task<Comment> CreateAComment(Comment comment, CancellationToken cancellationToken = default)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            if (comment.CreatedAt == default)
                comment.CreatedAt = DateTime.UtcNow;

            await _userContext.Comments.AddAsync(comment, cancellationToken);
            return comment;
        }
    }
}