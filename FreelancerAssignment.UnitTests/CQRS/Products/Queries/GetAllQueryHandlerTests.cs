using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FreelancerAssignment.CQRS.Products.Queries.GetAll;
using FreelancerAssignment.DTOs.Products;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using FreelancerAssignment.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FreelancerAssignment.UnitTests.CQRS.Products.Queries;

public class GetAllQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<GetAllQueryHandler>> _loggerMock = new();
    private readonly Mock<IUrlGenratorService> _urlGenratorServiceMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();

    public GetAllQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnProductResponses_WhenProductsExist()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product1", ProductCode = "P1", Price = 10, MinimumQuantity = 1 },
            new Product { Id = Guid.NewGuid(), Name = "Product2", ProductCode = "P2", Price = 20, MinimumQuantity = 2 }
        };
        _productRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);
        _urlGenratorServiceMock.Setup(u => u.GetImageUrl(It.IsAny<Guid>())).Returns<Guid>(id => $"url/{id}");

        var handler = new GetAllQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object, _urlGenratorServiceMock.Object);
        var query = new GetAllQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        foreach (var product in result.Value)
        {
            product.Image.Should().StartWith("url/");
        }
    }

    [Fact]
    public async Task Handle_Should_ReturnProductNotFound_WhenNoProductsExist()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync((List<Product>?)null);
        var handler = new GetAllQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object, _urlGenratorServiceMock.Object);
        var query = new GetAllQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.ProductNotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionThrown()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));
        var handler = new GetAllQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object, _urlGenratorServiceMock.Object);
        var query = new GetAllQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
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
