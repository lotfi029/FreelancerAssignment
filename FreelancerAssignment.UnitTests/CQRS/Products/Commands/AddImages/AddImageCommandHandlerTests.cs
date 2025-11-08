using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FreelancerAssignment.Abstractions;
using FreelancerAssignment.CQRS.Products.Commands.AddImages;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using FreelancerAssignment.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FreelancerAssignment.UnitTests.CQRS.Products.Commands.AddImages;

public class AddImageCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<AddImageCommandHandler>> _loggerMock = new();
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Mock<IFormFile> _formFileMock = new();

    public AddImageCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenImageIsAdded()
    {
        var product = new Product { Id = _productId, Image = null };
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _productId)).ReturnsAsync(product);
        _fileServiceMock.Setup(f => f.UploadImageAsync(_formFileMock.Object, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success("image-url"));
        _productRepoMock.Setup(r => r.UpdateAsync(product, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new AddImageCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _fileServiceMock.Object);
        var command = new AddImageCommand(_userId, _productId, _formFileMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("image-url");
        _productRepoMock.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnProductNotFound_WhenProductDoesNotExist()
    {
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _productId)).ReturnsAsync((Product?)null);
        var handler = new AddImageCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _fileServiceMock.Object);
        var command = new AddImageCommand(_userId, _productId, _formFileMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.ProductNotFound);
        _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenImageUploadFails()
    {
        var product = new Product { Id = _productId, Image = null };
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _productId)).ReturnsAsync(product);
        _fileServiceMock.Setup(f => f.UploadImageAsync(_formFileMock.Object, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure<string>(Error.BadRequest("UploadFailed", "Failed to upload")));
        var handler = new AddImageCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _fileServiceMock.Object);
        var command = new AddImageCommand(_userId, _productId, _formFileMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UploadFailed");
        _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionOccurs()
    {
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _productId)).ThrowsAsync(new Exception("Test exception"));
        var handler = new AddImageCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _fileServiceMock.Object);
        var command = new AddImageCommand(_userId, _productId, _formFileMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("error while add image");
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
