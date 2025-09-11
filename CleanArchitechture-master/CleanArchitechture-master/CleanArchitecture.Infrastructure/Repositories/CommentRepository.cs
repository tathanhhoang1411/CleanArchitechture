using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationContext _userContext;
        public CommentRepository(ApplicationContext userContext)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }
        public async Task<List<Comments>> DelListComment(int reviewId)
        {
            try
            {
                // Lấy danh sách các sản phẩm có ReviewID tương ứng
                var CommentToDelete = await _userContext.Comments
                    .Where(p => p.ReviewId == reviewId)
                    .ToListAsync();

                if (CommentToDelete.Count == 0)
                {
                    // Có thể ném ra ngoại lệ hoặc trả về danh sách rỗng tùy theo yêu cầu của bạn
                    return null; // Hoặc throw new Exception("No products found");
                }

                // Xóa tất cả các sản phẩm trong danh sách
                _userContext.Comments.RemoveRange(CommentToDelete);

                return CommentToDelete; // Trả về danh sách sản phẩm đã xóa
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<Comments>> GetListComment(int skip, int take, string str, long userID)
        {
            try
            {
                // Lấy danh sách các comment, bỏ qua 'skip' và lấy 'take' số lượng, lọc theo userID
                var listComment = await _userContext.Comments
                    .Where(p => p.UserId == userID && (string.IsNullOrEmpty(str) || p.CommentText.Contains(str)))
                    .OrderByDescending(p => p.CreatedAt) // Order by creation date descending
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();


                return listComment; // Trả về danh sách sản phẩm đã xóa
            }
            catch
            {
                return null;
            }
        }
    }
}