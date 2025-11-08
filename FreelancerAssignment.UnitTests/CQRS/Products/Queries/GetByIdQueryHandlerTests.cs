using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FreelancerAssignment.CQRS.Products.Queries.GetById;
using FreelancerAssignment.DTOs.Products;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using FreelancerAssignment.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FreelancerAssignment.UnitTests.CQRS.Products.Queries;

public class GetByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<GetByIdQueryHandler>> _loggerMock = new();
    private readonly Mock<IUrlGenratorService> _urlGenratorServiceMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Guid _productId = Guid.NewGuid();

    public GetByIdQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnProductResponse_WhenProductExists()
    {
        var product = new Product { Id = _productId, Name = "Product", ProductCode = "P1", Price = 10, MinimumQuantity = 1 };
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _urlGenratorServiceMock.Setup(u => u.GetImageUrl(_productId)).Returns("url/image");

        var handler = new GetByIdQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object, _urlGenratorServiceMock.Object);
        var query = new GetByIdQuery(_productId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Image.Should().Be("url/image");
    }

    [Fact]
    public async Task Handle_Should_ReturnProductNotFound_WhenProductDoesNotExist()
    {
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        var handler = new GetByIdQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object, _urlGenratorServiceMock.Object);
        var query = new GetByIdQuery(_productId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.ProductNotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionThrown()
    {
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _productId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));
        var handler = new GetByIdQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object, _urlGenratorServiceMock.Object);
        var query = new GetByIdQuery(_productId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("error while retrive the product");
        result.Error.Description.Should().Be("Test exception");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
