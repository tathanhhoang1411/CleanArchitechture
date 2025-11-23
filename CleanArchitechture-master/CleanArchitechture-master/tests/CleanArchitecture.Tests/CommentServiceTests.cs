using AutoMapper;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CleanArchitecture.Tests
{
    public class CommentServiceTests
    {
        // Khai báo Mocks cho tất cả các dependencies
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IRedisCacheService> _cacheMock = new();
        private readonly Mock<IRabbitMQService> _rabbitServiceMock = new();
        private readonly Mock<ILogger<CommentServices>> _loggerMock = new();

        // Khởi tạo Service
        private CommentServices _service;

        public CommentServiceTests()
        {
            _service = new CommentServices(
                null, // Giả sử tham số đầu tiên là null hoặc Mock
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _cacheMock.Object,
                _rabbitServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetList_Comment_ByOwner_Returns_MappedDtos()
        {
            // ARRANGE
            var comments = new List<Comment>
            {
                new Comment { CommentId = 1, CommentText = "a", ReviewId = 10, UserId = 100, CreatedAt = System.DateTime.UtcNow }
            };

            // Thiết lập Mock cho IUnitOfWork
            _unitOfWorkMock.Setup(u => u.Comments.GetListComment(0, 10, null, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comments)
                .Verifiable(); // Đánh dấu là có thể xác thực

            // Thiết lập Mock cho IMapper
            var dtoList = new List<CommentsDto> { new CommentsDto { CommentId = 1, CommentText = "a" } };
            _mapperMock.Setup(m => m.Map<List<CommentsDto>>(It.IsAny<List<Comment>>()))
                .Returns(dtoList)
                .Verifiable(); // Đánh dấu là có thể xác thực

            // ACT
            var result = await _service.GetList_Comment_ByOwner(0, 10, null, 100, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].CommentId);

            // Xác thực rằng các dependencies đã được gọi đúng cách
            _unitOfWorkMock.Verify(u => u.Comments.GetListComment(0, 10, null, 100, It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(m => m.Map<List<CommentsDto>>(It.IsAny<List<Comment>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetList_Comment_ByReviewID_Returns_Cached_Comments()
        {
            // ARRANGE
            int skip = 0;
            int take = 10;
            int reviewID = 1;
            var cacheKey = $"comments:reviewID:{reviewID}:skip:{skip}:take:{take}";
            var cachedCommentsDto = new List<CommentsDto> { new CommentsDto { CommentId = 1, CommentText = "Cached Comment" } };

            // Thiết lập Mock cho IRedisCacheService
            _cacheMock.Setup(c => c.GetAsync<List<CommentsDto>>(cacheKey))
                       .ReturnsAsync(cachedCommentsDto);

            // ACT
            var result = await _service.GetList_Comment_ByReviewID(skip, take, reviewID, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(cachedCommentsDto, result);
            _unitOfWorkMock.Verify(u => u.Comments.GetCommentsByIdReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetList_Comment_ByReviewID_Returns_Fetched_Comments_And_Caches_Them()
        {
            // ARRANGE
            int skip = 0;
            int take = 10;
            int reviewID = 1;
            var comments = new List<Comment> { new Comment { CommentId = 1, CommentText = "Fetched Comment" } };
            var commentsDto = new List<CommentsDto> { new CommentsDto { CommentId = 1, CommentText = "Fetched Comment" } };

            // Thiết lập Mock cho IUnitOfWork và trả về danh sách comment
            _unitOfWorkMock.Setup(u => u.Comments.GetCommentsByIdReview(skip, take, reviewID, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(comments);
            _mapperMock.Setup(m => m.Map<List<CommentsDto>>(comments)).Returns(commentsDto);

            // ACT
            var result = await _service.GetList_Comment_ByReviewID(skip, take, reviewID, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(commentsDto, result);

            // Xác thực cache được gọi để lưu trữ dữ liệu
            _cacheMock.Verify(c => c.SetAsync(
                It.Is<string>(key => key == $"comments:reviewID:{reviewID}:skip:{skip}:take:{take}"),
                comments,
                TimeSpan.FromMinutes(1)), Times.Once);
        }
    }
}