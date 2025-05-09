using AutoMapper;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
            return await  _userContext.SaveChangesAsync();
        }

        public async Task<List<ReviewDto>> GetListReviews(int skip, int take, string data)
        {
            var products = await _userContext.Reviews
                .Where(p => p.ReviewText.Contains(data))
                .Take(take)
                .Skip(skip)
                .OrderBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<List<ReviewDto>>(products);
        }
    }
}
