using FluentAssertions;
using FreelancerAssignment.CQRS.Products.Commands.Update;
using FreelancerAssignment.DTOs.Products;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelancerAssignment.UnitTests.CQRS.Products.Commands.Update;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateProductCommandHandler>> _loggerMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Product _existingProduct;
    private readonly UpdateProductRequest _updateRequest;
    private readonly UpdateProductCommand _validCommand;
    private readonly Guid _userId;

    public UpdateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new();
        _loggerMock = new();
        _productRepoMock = new();
        _userId = Guid.NewGuid();
        _existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            Category = "Old Category",
            ProductCode = "OLD-001",
            Image = "old.jpg",
            Price = 10.0m,
            MinimumQuantity = 1,
            CreatedById = _userId
        };
        _updateRequest = new UpdateProductRequest(
            Name: "New Name",
            Category: "New Category",
            Price: 20.0m,
            MinimumQuantity: 2,
            Discount: 5
        );

        _validCommand = new UpdateProductCommand(_userId, _existingProduct.Id, _updateRequest);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenProductExistsAndUserIsOwner()
    {
        _productRepoMock.Setup(
            r => r.FindAsync(
                It.IsAny<CancellationToken>(), 
                _existingProduct.Id)
            ).ReturnsAsync(_existingProduct);

        var handler = new UpdateProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);

        var result = await handler.Handle(_validCommand, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _productRepoMock.Verify(x => x.UpdateAsync(It.Is<Product>(p => p.Name == _updateRequest.Name), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenProductNotFound()
    {
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _existingProduct.Id))
            .ReturnsAsync((Product?)null);
        var handler = new UpdateProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);

        var result = await handler.Handle(_validCommand, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.ProductNotFound);
        _productRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserIsNotOwner()
    {
        var notOwnerId = Guid.NewGuid();
        var command = new UpdateProductCommand(notOwnerId, _existingProduct.Id, _updateRequest);
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _existingProduct.Id))
            .ReturnsAsync(_existingProduct);
        var handler = new UpdateProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.UnauthorizedAccess);
        _productRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionOccurs()
    {
        var expectedException = new Exception("Test exception");
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _existingProduct.Id))
            .ThrowsAsync(expectedException);
        var handler = new UpdateProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);

        var result = await handler.Handle(_validCommand, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UpdateProductError");
        result.Error.Description.Should().Be(expectedException.Message);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
