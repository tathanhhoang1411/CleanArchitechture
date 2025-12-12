using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationContext _userContext;
        private readonly int _maxTake;

        public ReviewRepository(ApplicationContext userContext, IConfiguration configuration)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var configured = configuration.GetValue<int?>("Paging:MaxTake");
            _maxTake = (configured.HasValue && configured.Value > 0) ? configured.Value : 5000;
        }

        public async Task<long> DelReview(int reviewId, CancellationToken cancellationToken = default)
        {
            try
            {
                var aReview = await _userContext.Reviews
                    .FirstOrDefaultAsync(p => p.ReviewId == reviewId, cancellationToken);

                if (aReview == null)
                {
                    return -1;
                }

                _userContext.Reviews.Remove(aReview);

                return aReview.ReviewId;
            }
            catch
            {
                return -1;
            }
        }

        public async Task<Review> CreateReview(Review createReview, CancellationToken cancellationToken = default)
        {
            if (createReview == null) return null;

            try
            {
                await _userContext.Reviews.AddAsync(createReview, cancellationToken);
                return createReview;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<object>> GetListReviews(int skip, int take, string str, long userID, CancellationToken cancellationToken = default)
        {
            // 1. Đảm bảo tính toàn vẹn của tham số phân trang
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            // Giả định _maxTake là một hằng số hoặc trường trong lớp
            take = Math.Min(take, _maxTake);

            try
            {
                // 2. Bắt đầu xây dựng IQueryable (Dynamic Query Construction)
                // Bắt đầu với AsNoTracking() để chỉ định đây là truy vấn chỉ đọc, tăng hiệu suất.
                IQueryable<Review> reviewQuery = _userContext.Reviews.AsNoTracking();

                // 3. Áp dụng điều kiện lọc theo UserID (nếu có)
                if (userID > 0)
                {
                    reviewQuery = reviewQuery.Where(r => r.OwnerID == userID);
                }

                // 4. Áp dụng điều kiện lọc Tìm kiếm (Chỉ khi str có giá trị)
                if (!string.IsNullOrEmpty(str))
                {
                    // Tối ưu hóa: Loại bỏ EndsWith(str) (LIKE '%str') nếu không bắt buộc, 
                    // vì nó làm truy vấn rất chậm (Table Scan).
                    // Thay vào đó, nếu muốn tìm kiếm linh hoạt hơn, dùng Contains.

                    // Lựa chọn 1: Giữ lại tìm kiếm bắt đầu bằng (Performance tốt hơn EndsWith)
                    reviewQuery = reviewQuery.Where(r => r.ReviewText.StartsWith(str));

                    // Lựa chọn 2: Nếu muốn tìm kiếm nội dung bất kỳ, dùng Contains (LIKE '%str%'), nhưng vẫn nên đánh index hoặc dùng Full-Text Search
                    // reviewQuery = reviewQuery.Where(r => r.ReviewText.Contains(str));

                    // Nếu bạn bắt buộc phải dùng StartsWith HOẶC EndsWith:
                    // reviewQuery = reviewQuery.Where(r => r.ReviewText.StartsWith(str) || r.ReviewText.EndsWith(str));
                }

                // 5. Kết hợp (JOIN), Sắp xếp, Phân trang và Chọn dữ liệu (Projection) MỘT LẦN
                var reviewList = await (
                    from review in reviewQuery // Bắt đầu từ IQueryable đã lọc
                    orderby review.CreatedAt descending

                    // Left Join với Products. Cũng nên AsNoTracking() cho Products nếu có thể.
                    join product in _userContext.Products.AsNoTracking()
                        on review.ReviewId equals product.ReviewID into productGroup
                    from product in productGroup.DefaultIfEmpty()

                        // Chọn các thuộc tính cần thiết (Projection)
                    select new
                    {
                        review.ReviewId,
                        review.Rating,
                        review.ReviewText,
                        review.CreatedAt,
                        ProductName = product != null ? product.ProductName : null,
                        Price = product != null ? product.Price : (decimal?)null,
                        ProductImage1 = product != null ? product.ProductImage1 : null,
                        ProductImage2 = product != null ? product.ProductImage2 : null,
                        ProductImage3 = product != null ? product.ProductImage3 : null,
                        ProductImage4 = product != null ? product.ProductImage4 : null,
                        ProductImage5 = product != null ? product.ProductImage5 : null
                    })
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync(cancellationToken);

                // 6. Chuyển đổi sang ExpandoObject (giữ nguyên vì chữ ký hàm yêu cầu List<object>)
                var result = new List<object>();
                foreach (var r in reviewList)
                {
                    dynamic expando = new ExpandoObject();
                    // Sử dụng IDictionary<string, object> để gán thuộc tính sẽ giúp code ngắn hơn,
                    // nhưng tôi giữ lại cấu trúc hiện tại vì nó quen thuộc.
                    expando.ReviewId = r.ReviewId;
                    expando.Rating = r.Rating;
                    expando.ReviewText = r.ReviewText;
                    expando.CreatedAt = r.CreatedAt;
                    expando.ProductName = r.ProductName;
                    expando.Price = r.Price;
                    expando.ProductImage1 = r.ProductImage1;
                    expando.ProductImage2 = r.ProductImage2;
                    expando.ProductImage3 = r.ProductImage3;
                    expando.ProductImage4 = r.ProductImage4;
                    expando.ProductImage5 = r.ProductImage5;

                    result.Add(expando);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Ghi Log lỗi ở đây để theo dõi các vấn đề trong CSDL
                // Ví dụ: _logger.LogError(ex, "Lỗi khi lấy danh sách reviews.");
                return new List<object>();
            }
        }

        public async Task<List<Review>> GetListReviewsByOwnerID(int reviewID, long ownerID, CancellationToken cancellationToken = default)
        {
            try
            {
                var list = await (
                    from review in _userContext.Reviews
                    where review.ReviewId == reviewID && review.OwnerID == ownerID
                    orderby review.CreatedAt descending
                    select review)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return list ?? new List<Review>();
            }
            catch
            {
                return new List<Review>();
            }
        }
    }
}
