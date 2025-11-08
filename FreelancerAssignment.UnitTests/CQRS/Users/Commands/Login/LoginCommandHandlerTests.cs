using FluentAssertions;
using FreelancerAssignment.CQRS.Users.Commands.Login;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using FreelancerAssignment.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelancerAssignment.UnitTests.CQRS.Users.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly User _user;
    private readonly string _password = "TestPassword123!";
    private readonly string _hashedPassword;

    public LoginCommandHandlerTests()
    {
        _user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = null!,
            RefreshTokens = new System.Collections.Generic.List<RefreshToken>()
        };
        var hasher = new PasswordHasher<User>();
        _hashedPassword = hasher.HashPassword(_user, _password);
        _user.PasswordHash = _hashedPassword;
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenCredentialsAreValid()
    {
        _userRepoMock.Setup(r => r.GetUserByEmailOrUsername(_user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        _jwtProviderMock.Setup(j => j.GenerateToken(_user)).Returns(("token", 3600));
        var handler = new LoginCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new LoginCommand(_user.Username, _password);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be(_user.Username);
        result.Value.Email.Should().Be(_user.Email);
        result.Value.Token.Should().Be("token");
        result.Value.ExpiresIn.Should().Be(3600);
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshTokenExpiration.Should().BeAfter(DateTime.UtcNow);
        _userRepoMock.Verify(r => r.UpdateAsync(_user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidCredentials_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetUserByEmailOrUsername(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var handler = new LoginCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new LoginCommand("notfound", _password);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidCredentials_WhenPasswordIsWrong()
    {
        _userRepoMock.Setup(r => r.GetUserByEmailOrUsername(_user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        var handler = new LoginCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new LoginCommand(_user.Username, "WrongPassword");
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionOccurs()
    {
        _userRepoMock.Setup(r => r.GetUserByEmailOrUsername(_user.Username, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));
        var handler = new LoginCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new LoginCommand(_user.Username, _password);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("LoginUserError");
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
