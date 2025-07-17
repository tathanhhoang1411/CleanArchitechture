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
        private readonly IMapper _mapper;
        public ReviewRepository(ApplicationContext userContext, IMapper mapper)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<int> CreateReview(Reviews createReview)
        {
            _userContext.Add(createReview);
            return await _userContext.SaveChangesAsync();
        }

        public class QueryReview
        {
            public string? str { get; set; }
            public int userID { get; set; }
        }
        public async Task<List<object>> GetListReviews(int skip, int take, string str, long userID)
        {

            var reviewList = await (
                from review in _userContext.Reviews
                where review.ReviewText.Contains(str) && review.OwnerID == userID  // Lọc trước khi join
                join product in _userContext.Products on review.ReviewId equals product.ReviewID // Đảm bảo dùng đúng khóa
                orderby review.CreatedAt descending
                select new
                {
                    review.ReviewId,
                    review.Rating,
                    review.ReviewText,
                    review.CreatedAt,
                    ProductName = product.ProductName,
                    product.Price,
                    ProductImage1 = product.ProductImage1,
                    ProductImage2 = product.ProductImage2,
                    ProductImage3 = product.ProductImage3,
                    ProductImage4 = product.ProductImage4,
                    ProductImage5 = product.ProductImage5
                }
            )
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();

            // Ánh xạ qua ExpandoObject
            var mappedReviewList = reviewList.Select(r =>
            {
                dynamic expando = new ExpandoObject();
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

                return expando;
            }).ToList();

            // Trả về danh sách đã ánh xạ
            return mappedReviewList;
        }
    }
}
