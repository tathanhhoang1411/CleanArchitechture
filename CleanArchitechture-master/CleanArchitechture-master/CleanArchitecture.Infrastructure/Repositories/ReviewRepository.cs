using AutoMapper;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CleanArchitecture.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private ApplicationContext _userContext;
        public ReviewRepository(ApplicationContext userContext)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }
        public async Task<int> DelReview(int reviewId)
        {
            Reviews? aReview = null;
            try
            {
                 aReview = await _userContext.Reviews
    .AsNoTracking()
    .FirstOrDefaultAsync(p => p.ReviewId == reviewId);

                if (aReview ==null)
                {
                    // Có thể ném ra ngoại lệ hoặc trả về null tùy theo yêu cầu của bạn
                    return -1; // hoặc throw new Exception("Product not found");
                }

                _userContext.Reviews.Remove(aReview);

                return aReview.ReviewId;
            }
            catch
            {
                return -1;
            }
        }
        public async Task<Reviews> CreateReview(Reviews createReview)
        {
            Reviews? reviews = null;
            try
            {
                await _userContext.AddAsync(createReview);
                return createReview;
            }
            catch
            {
                return reviews;

            }
        }

        public async Task<List<object>> GetListReviews(int skip, int take, string str, long userID)
        {
            List<object> list = null;
            IEnumerable<dynamic> reviewList = new List<dynamic>(); ;
            try
            {
                switch (userID)
                {
                    case 0:
                         reviewList = await (
from review in _userContext.Reviews
where review.ReviewText.Contains(str) 
join product in _userContext.Products on review.ReviewId equals product.ReviewID into productGroup
from product in productGroup.DefaultIfEmpty() // Thực hiện left outer join
orderby review.CreatedAt descending
select new
{
 review.ReviewId,
 review.Rating,
 review.ReviewText,
 review.CreatedAt,
 ProductName = product != null ? product.ProductName : null,
 Price = product != null ? product.Price : (decimal?)null, // Chuyển sang nullable type
 ProductImage1 = product != null ? product.ProductImage1 : null,
 ProductImage2 = product != null ? product.ProductImage2 : null,
 ProductImage3 = product != null ? product.ProductImage3 : null,
 ProductImage4 = product != null ? product.ProductImage4 : null,
 ProductImage5 = product != null ? product.ProductImage5 : null
}
)
.Skip(skip)
.Take(take)
.AsNoTracking()
.ToListAsync();
                        break;

                    case >0:
                         reviewList = await (
from review in _userContext.Reviews
where review.ReviewText.Contains(str) && review.OwnerID == userID
join product in _userContext.Products on review.ReviewId equals product.ReviewID into productGroup
from product in productGroup.DefaultIfEmpty() // Thực hiện left outer join
orderby review.CreatedAt descending
select new
{
 review.ReviewId,
 review.Rating,
 review.ReviewText,
 review.CreatedAt,
 ProductName = product != null ? product.ProductName : null,
 Price = product != null ? product.Price : (decimal?)null, // Chuyển sang nullable type
 ProductImage1 = product != null ? product.ProductImage1 : null,
 ProductImage2 = product != null ? product.ProductImage2 : null,
 ProductImage3 = product != null ? product.ProductImage3 : null,
 ProductImage4 = product != null ? product.ProductImage4 : null,
 ProductImage5 = product != null ? product.ProductImage5 : null
}
)
.Skip(skip)
.Take(take)
.AsNoTracking()
.ToListAsync();
                        break;
                }

                // Ánh xạ qua ExpandoObject

                List<dynamic> mappedReviewList = new List<dynamic>();

                foreach (var r in reviewList)
                {
                    dynamic expando = new ExpandoObject() as IDictionary<string, object>;
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

                    mappedReviewList.Add(expando);
                }
                // Trả về danh sách đã ánh xạ
                return mappedReviewList;
            }
            catch
            {
                return list;
            }
        }
        public async Task<List<Reviews>> GetListReviewsByOwnerID(int reviewID, long ownerID)
        {
            List<Reviews> list = null;
            try
            {
                 list = await (
                from review in _userContext.Reviews
                where review.ReviewId == reviewID && review.OwnerID == ownerID  // Lọc trước khi join
                orderby review.CreatedAt descending
                select review) // Thêm select để lấy danh sách đánh giá)
           .AsNoTracking()
            .ToListAsync();
                if (list.Count() == 0)
                {
                    return null;
                }
                // Trả về danh sách đã ánh xạ
                return list;
            }
            catch
            {
                return list;
            }
        }
    }
}
