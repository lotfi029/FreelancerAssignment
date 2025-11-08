using FluentAssertions;
using FreelancerAssignment.CQRS.Products.Commands.Add;
using FreelancerAssignment.DTOs.Products;
using FreelancerAssignment.Entities;
using FreelancerAssignment.IRepositories;
using FreelancerAssignment.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelancerAssignment.UnitTests.CQRS.Products.Commands.Add;

public class AddProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<AddProductCommandHandler>> _loggerMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IFileService> _fileService = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly ProductRequest _productRequest;

    public AddProductCommandHandlerTests()
    {
        _productRequest = new ProductRequest(
            Name: "Test Product",
            Category: "Test Category",
            Price: 100.0m,
            MinimumQuantity: 1,
            Discount: 10,
            Image: Mock.Of<IFormFile>()
        );
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenProductIsAdded()
    {
        _productRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        _unitOfWorkMock.Setup(u => u.CommitChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new AddProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _fileService.Object);
        var command = new AddProductCommand(_userId, _productRequest);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
