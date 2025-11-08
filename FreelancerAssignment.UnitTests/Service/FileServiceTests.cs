using System.Text;
using FluentAssertions;
using FreelancerAssignment.Abstractions;
using FreelancerAssignment.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelancerAssignment.UnitTests.Service;

public class FileServiceTests
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly Mock<ILogger<FileService>> _loggerMock = new();
    private readonly string _webRootPath;
    private readonly FileService _sut;

    public FileServiceTests()
    {
        _webRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_webRootPath);
        _envMock.Setup(e => e.WebRootPath).Returns(_webRootPath);
        _sut = new FileService(_envMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UploadImageAsync_Should_SaveFile_AndReturnName()
    {
        // Arrange
        var fileName = "test.jpg";
        var fileContent = Encoding.UTF8.GetBytes("dummy image content");
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns(fileName);
        formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((stream, ct) => stream.WriteAsync(fileContent, 0, fileContent.Length, ct));

        // Act
        var result = await _sut.UploadImageAsync(formFileMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().EndWith(".jpg");
        var savedPath = Path.Combine(_webRootPath, ImageSettings.ImagePath, result.Value);
        File.Exists(savedPath).Should().BeTrue();
    }

    [Fact]
    public async Task StreamAsync_Should_ReturnFileStream_WhenFileExists()
    {
        // Arrange
        var fileName = "streamtest.jpg";
        var imageDir = Path.Combine(_webRootPath, ImageSettings.ImagePath);
        Directory.CreateDirectory(imageDir);
        var filePath = Path.Combine(imageDir, fileName);
        await File.WriteAllTextAsync(filePath, "image data");

        // Act
        var result = await _sut.StreamAsync(fileName);

        // Assert
        result.stream.Should().NotBeNull();
        result.contentType.Should().Be("image/jpeg");
        result.fileName.Should().Be(fileName);
        result.stream?.Dispose();
    }

    [Fact]
    public async Task StreamAsync_Should_ReturnDefault_WhenFileDoesNotExist()
    {
        // Act
        var result = await _sut.StreamAsync("nonexistent.jpg");

        // Assert
        result.stream.Should().BeNull();
        result.contentType.Should().BeNull();
        result.fileName.Should().BeNull();
    }

    [Fact]
    public async Task DeleteImageAsync_Should_DeleteFile_WhenExists()
    {
        // Arrange
        var fileName = "delete.jpg";
        var imageDir = Path.Combine(_webRootPath, ImageSettings.ImagePath);
        Directory.CreateDirectory(imageDir);
        var filePath = Path.Combine(imageDir, fileName);
        await File.WriteAllTextAsync(filePath, "image data");

        // Act
        var result = await _sut.DeleteImageAsync(fileName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImageAsync_Should_ReturnSuccess_WhenFileDoesNotExist()
    {
        // Act
        var result = await _sut.DeleteImageAsync("missing.jpg");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose()
    {
        if (Directory.Exists(_webRootPath))
        {
            Directory.Delete(_webRootPath, true);
        }
    }
}
