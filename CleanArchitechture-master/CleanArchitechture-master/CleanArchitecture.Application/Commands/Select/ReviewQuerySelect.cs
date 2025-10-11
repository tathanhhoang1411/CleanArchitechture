using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Commands.Select
{

    public class ReviewQuerySelect : IRequest<List<object>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public QueryEF Data { get; set; }

        public ReviewQuerySelect(int skip, int take, QueryEF data)
        {
            Skip = skip;
            Take = take;
            Data = data;
        }
        public class ReviewQuerySelectHandler : IRequestHandler<ReviewQuerySelect, List<object>>
        {
            private readonly IReviewServices _reviewServices;
            public ReviewQuerySelectHandler(IReviewServices reviewServices)
            {
                _reviewServices = reviewServices ?? throw new ArgumentNullException(nameof(reviewServices));
            }
            public async Task<List<object>> Handle(ReviewQuerySelect query, CancellationToken cancellationToken)
            {
                try
                {
                    var ReviewDtoList = await _reviewServices.GetList_Reviews(query.Skip, query.Take, query.Data.str, query.Data.ID);
                    return ReviewDtoList ?? new List<object>(); // Trả về danh sách rỗng nếu không có bài review
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}

