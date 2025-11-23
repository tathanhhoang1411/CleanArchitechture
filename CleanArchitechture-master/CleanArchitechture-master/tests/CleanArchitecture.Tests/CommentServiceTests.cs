using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.Tests
{
    public class CommentServiceTests
    {
        // Khai báo Mocks cho tất cả các dependencies, bao gồm RabbitMQService
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IRedisCacheService> _cacheMock = new();
        // SỬA ĐỔI: Sử dụng Mock cho RabbitMQService (giả định có IRabbitMQService)
        private readonly Mock<IRabbitMQService> _rabbitServiceMock = new();
        private readonly Mock<ILogger<CommentServices>> _loggerMock = new();

        // LOẠI BỎ: Không cần hàm khởi tạo để thiết lập cấu hình RabbitMQ nữa
        public CommentServiceTests()
        {
            // Nếu CommentServices cần IConfiguration, bạn cũng nên Mock nó
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

            // Khởi tạo Service, truyền các Mock Objects vào (thay vì null và RabbitMQService thật)
            var service = new CommentServices(
                null, // Giả sử tham số đầu tiên là null hoặc Mock
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _cacheMock.Object,
                _rabbitServiceMock.Object, // Dùng Mock Object
                _loggerMock.Object);

            // ACT
            var result = await service.GetList_Comment_ByOwner(0, 10, null, 100, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].CommentId);

            // BỔ SUNG: Xác thực rằng các dependencies đã được gọi đúng cách (Verification)

            // 1. Xác thực Repository đã được gọi chính xác MỘT lần
            _unitOfWorkMock.Verify(u => u.Comments.GetListComment(0, 10, null, 100, It.IsAny<CancellationToken>()),
                Times.Once, "GetListComment phải được gọi chính xác 1 lần với các tham số này.");

            // 2. Xác thực Mapper đã được gọi chính xác MỘT lần
            _mapperMock.Verify(m => m.Map<List<CommentsDto>>(It.IsAny<List<Comment>>()),
                Times.Once, "Map phải được gọi chính xác 1 lần để ánh xạ dữ liệu.");

            // Nếu có các tương tác khác (ví dụ: logger, cache), bạn cũng nên verify chúng.
        }
    }
}
