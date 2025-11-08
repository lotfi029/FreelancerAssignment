using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FreelancerAssignment.CQRS.Users.Queries.Profile;
using FreelancerAssignment.DTOs.Users;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FreelancerAssignment.UnitTests.CQRS.Users.Queries.Profile;

public class GetProfileQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<GetProfileQueryHandler>> _loggerMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Guid _userId = Guid.NewGuid();

    public GetProfileQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnProfileResponse_WhenUserExists()
    {
        var user = new User { Id = _userId, Username = "testuser", Email = "test@example.com" };
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _userId)).ReturnsAsync(user);
        var handler = new GetProfileQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        var query = new GetProfileQuery(_userId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be("testuser");
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_Should_ReturnUserNotFound_WhenUserDoesNotExist()
    {
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _userId)).ReturnsAsync((User?)null);
        var handler = new GetProfileQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        var query = new GetProfileQuery(_userId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UserNotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionThrown()
    {
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _userId)).ThrowsAsync(new Exception("Test exception"));
        var handler = new GetProfileQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        var query = new GetProfileQuery(_userId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Get Profile Exception");
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
