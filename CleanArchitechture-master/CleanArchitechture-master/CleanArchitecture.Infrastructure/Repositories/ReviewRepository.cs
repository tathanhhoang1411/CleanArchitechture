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

        public async Task<int> DelReview(int reviewId, CancellationToken cancellationToken = default)
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
                if (createReview.CreatedAt == default)
                    createReview.CreatedAt = DateTime.UtcNow;

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
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            take = Math.Min(take, _maxTake);

            try
            {
                var result = new List<object>();

                if (userID == 0)
                {
                    var reviewList = await (
                        from review in _userContext.Reviews
                        where string.IsNullOrEmpty(str) || review.ReviewText.StartsWith(str) || review.ReviewText.EndsWith(str)
                        join product in _userContext.Products on review.ReviewId equals product.ReviewID into productGroup
                        from product in productGroup.DefaultIfEmpty()
                        orderby review.CreatedAt descending
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
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);

                    foreach (var r in reviewList)
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

                        result.Add(expando);
                    }
                }
                else if (userID > 0)
                {
                    var reviewList = await (
                        from review in _userContext.Reviews
                        where review.OwnerID == userID && (string.IsNullOrEmpty(str) || review.ReviewText.StartsWith(str) || review.ReviewText.EndsWith(str))
                        join product in _userContext.Products on review.ReviewId equals product.ReviewID into productGroup
                        from product in productGroup.DefaultIfEmpty()
                        orderby review.CreatedAt descending
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
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);

                    foreach (var r in reviewList)
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

                        result.Add(expando);
                    }
                }

                return result;
            }
            catch
            {
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
