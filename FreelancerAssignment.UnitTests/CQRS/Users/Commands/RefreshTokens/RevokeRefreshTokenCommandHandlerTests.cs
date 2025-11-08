using FluentAssertions;
using FreelancerAssignment.Authentication;
using FreelancerAssignment.CQRS.Users.Commands.RefreshTokens;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelancerAssignment.UnitTests.CQRS.Users.Commands.RefreshTokens;

public class RevokeRefreshTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly User _user;
    private readonly RefreshToken _refreshToken;
    private readonly string _token = "token";
    private readonly string _refreshTokenValue = "refreshTokenValue";

    public RevokeRefreshTokenCommandHandlerTests()
    {
        _user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            RefreshTokens = []
        };
        _refreshToken = new RefreshToken
        {
            Token = _refreshTokenValue,
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };
        _user.RefreshTokens.Add(_refreshToken);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenTokenAndRefreshTokenAreValid()
    {
        _jwtProviderMock.Setup(j => j.ValidateToken(_token)).Returns(_user.Id);
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _user.Id)).ReturnsAsync(_user);
        var handler = new RevokeRefreshTokenCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RevokeRefreshTokenCommand(_token, _refreshTokenValue);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        _refreshToken.RevokeOn.Should().NotBeNull();
        _userRepoMock.Verify(r => r.UpdateAsync(_user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidToken_WhenTokenIsInvalid()
    {
        _jwtProviderMock.Setup(j => j.ValidateToken(_token)).Returns(Guid.Empty);
        var handler = new RevokeRefreshTokenCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RevokeRefreshTokenCommand(_token, _refreshTokenValue);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidUserId_WhenUserNotFound()
    {
        _jwtProviderMock.Setup(j => j.ValidateToken(_token)).Returns(_user.Id);
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _user.Id)).ReturnsAsync((User?)null);
        var handler = new RevokeRefreshTokenCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RevokeRefreshTokenCommand(_token, _refreshTokenValue);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidUserId);
    }

    [Fact]
    public async Task Handle_Should_ReturnNoRefreshToken_WhenRefreshTokenIsInvalid()
    {
        _jwtProviderMock.Setup(j => j.ValidateToken(_token)).Returns(_user.Id);
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<CancellationToken>(), _user.Id)).ReturnsAsync(_user);
        var handler = new RevokeRefreshTokenCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RevokeRefreshTokenCommand(_token, "invalidToken");
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NoRefreshToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionOccurs()
    {
        _jwtProviderMock.Setup(j => j.ValidateToken(_token)).Throws(new Exception("Test exception"));
        var handler = new RevokeRefreshTokenCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RevokeRefreshTokenCommand(_token, _refreshTokenValue);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RevokeRefreshTokenError");
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
