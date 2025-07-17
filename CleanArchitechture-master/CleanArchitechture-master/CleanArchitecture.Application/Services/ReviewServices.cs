using AutoMapper;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Repository
{
    public class ReviewServices : IReviewServices
    {
        private readonly IReviewRepository _reviewRepository;
        public ReviewServices(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        }

        public Task<List<object>> GetList_Reviews(int skip, int take, string str, long userID)
        {
            return _reviewRepository.GetListReviews(skip,take, str, userID);
        }

        public Task<int> Review_Create(Reviews review)
        {
            return _reviewRepository.CreateReview(review);
        }

    }
}
