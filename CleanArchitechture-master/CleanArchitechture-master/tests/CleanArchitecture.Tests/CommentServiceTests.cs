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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IRedisCacheService> _cacheMock = new();
        private readonly RabbitMQService _rabbitService;
        private readonly Mock<ILogger<CommentServices>> _loggerMock = new();

        public CommentServiceTests()
        {
            var inMemoryConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "RabbitMQ:Host", "localhost" },
                    { "RabbitMQ:Username", "guest" },
                    { "RabbitMQ:Password", "guest" }
                })
                .Build();

            _rabbitService = new RabbitMQService(inMemoryConfig);
        }

        [Fact]
        public async Task GetList_Comment_ByOwner_Returns_MappedDtos()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new Comment { CommentId = 1, CommentText = "a", ReviewId = 10, UserId = 100, CreatedAt = System.DateTime.UtcNow }
            };

            _unitOfWorkMock.Setup(u => u.Comments.GetListComment(0, 10, null, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comments);

            var dtoList = new List<CommentsDto> { new CommentsDto { CommentId = 1, CommentText = "a" } };
            _mapperMock.Setup(m => m.Map<List<CommentsDto>>(It.IsAny<List<Comment>>())).Returns(dtoList);

            var service = new CommentServices(null, _unitOfWorkMock.Object, _mapperMock.Object, _cacheMock.Object, _rabbitService, _loggerMock.Object);

            // Act
            var result = await service.GetList_Comment_ByOwner(0, 10, null, 100, CancellationToken.None);

            // Assert: xác th?c k?t qu? tr? v? phù h?p mong ð?i
            Assert.NotNull(result); // K?t qu? không null
            Assert.Single(result); // Ch? có 1 ph?n t? trong danh sách
            Assert.Equal(1, result[0].CommentId); // Ki?m tra ID c?a ph?n t? tr? v?
        }
    }
}
