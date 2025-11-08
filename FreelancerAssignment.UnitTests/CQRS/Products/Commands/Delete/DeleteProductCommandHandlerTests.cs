using FluentAssertions;
using FreelancerAssignment.CQRS.Products.Commands.Delete;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelancerAssignment.UnitTests.CQRS.Products.Commands.Delete;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<DeleteProductCommandHandler>> _loggerMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Product _existingProduct;
    private readonly Guid _userId;
    private readonly DeleteProductCommand _validCommand;

    public DeleteProductCommandHandlerTests()
    {
        _unitOfWorkMock = new();
        _loggerMock = new();
        _productRepoMock = new();
        _userId = Guid.NewGuid();
        _existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Category = "Test Category",
            ProductCode = "TEST-001",
            Image = "test.jpg",
            Price = 10.0m,
            MinimumQuantity = 1,
            DeletedAt = null,
            CreatedById = _userId
        };
        _validCommand = new DeleteProductCommand(_userId, _existingProduct.Id);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_WhenUnitOfWorkIsNull()
    {
        var action = () => new DeleteProductCommandHandler(null!, _loggerMock.Object);
        
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("unitOfWork");
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_WhenLoggerIsNull()
    {
        var action = () => new DeleteProductCommandHandler(_unitOfWorkMock.Object, null!);
        
        action.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenProductExistsAndUserIsOwner()
    {
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _existingProduct.Id))
            .ReturnsAsync(_existingProduct);

        var handler = new DeleteProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);

        var result = await handler.Handle(_validCommand, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _existingProduct.DeletedAt.Should().NotBeNull();
        
        _productRepoMock.Verify(
            x => x.UpdateAsync(_existingProduct, It.IsAny<CancellationToken>()),
            Times.Once);
        
        _unitOfWorkMock.Verify(
            x => x.CommitChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenProductNotFound()
    {
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _existingProduct.Id))
            .ReturnsAsync((Product?)null);

        var handler = new DeleteProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);

        var result = await handler.Handle(_validCommand, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.ProductNotFound);
        
        _productRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _unitOfWorkMock.Verify(
            x => x.CommitChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnauthorizedAccess_WhenUserIsNotOwner()
    {
        var differentUserId = Guid.NewGuid();
        var command = new DeleteProductCommand(differentUserId, _existingProduct.Id);

        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _existingProduct.Id))
            .ReturnsAsync(_existingProduct);

        var handler = new DeleteProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.UnauthorizedAccess);
        
        _productRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _unitOfWorkMock.Verify(
            x => x.CommitChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionOccurs()
    {
        var expectedException = new Exception("Test exception");
        
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _existingProduct.Id))
            .ThrowsAsync(expectedException);

        var handler = new DeleteProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);

        var result = await handler.Handle(_validCommand, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeleteProductError");
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

    [Fact]
    public async Task Handle_Should_SetDeletedAtToCurrentTime_WhenDeletingProduct()
    {
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _existingProduct.Id))
            .ReturnsAsync(_existingProduct);

        var handler = new DeleteProductCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        var beforeDelete = DateTime.Now;

        var result = await handler.Handle(_validCommand, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _existingProduct.DeletedAt.Should().NotBeNull();
        _existingProduct.DeletedAt.Should().BeOnOrAfter(beforeDelete);
        _existingProduct.DeletedAt.Should().BeOnOrBefore(DateTime.Now);
    }
}
