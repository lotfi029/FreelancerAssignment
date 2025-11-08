using FluentAssertions;
using FreelancerAssignment.Authentication;
using FreelancerAssignment.CQRS.Users.Commands.Register;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace FreelancerAssignment.UnitTests.CQRS.Users.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    public RegisterCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenRegistrationIsValid()
    {
        _userRepoMock.Setup(
            r => r.ExistsAsync(
                e => e.Username == "testuser", 
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(false);

        _userRepoMock.Setup(
            r => r.ExistsAsync(
                e => e.Email == "test@example.com", 
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(false);

        _userRepoMock.Setup(
            r => r.AddAsync(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(Guid.NewGuid());

        _unitOfWorkMock.Setup(
            u => u.CommitChangesAsync(
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(1);

        var handler = new RegisterCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RegisterCommand("testuser", "TestPassword123!", "test@example.com");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnUsernameNotUnique_WhenUsernameExists()
    {
        _userRepoMock.Setup(
            e => e.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync((Expression<Func<User, bool>> expr, CancellationToken _) =>
            {
                var compiled = expr.Compile();
                return compiled(new User { Username = "testuser", Email = "other@example.com" });
            });

        var handler = new RegisterCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RegisterCommand("testuser", "TestPassword123!", "test@example.com");

        var result = await handler.Handle(command, CancellationToken.None);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.UsernameNotUnique);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmailNotUnique_WhenEmailExists()
    {

        _userRepoMock.Setup(
            e => e.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>())
            ).ReturnsAsync((Expression<Func<User, bool>> expr, CancellationToken _) =>
            {
                var compiled = expr.Compile();
                return compiled(new User { Username = "otheruser", Email = "test@example.com" });
            });

        var handler = new RegisterCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RegisterCommand("testuser", "TestPassword123!", "test@example.com");
        var result = await handler.Handle(command, default);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmailNotUnique);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExceptionOccurs()
    {
        _userRepoMock.Setup(
            r => r.ExistsAsync(
                It.IsAny<Expression<Func<User, bool>>>(), 
                It.IsAny<CancellationToken>())
            ).ThrowsAsync(new Exception("Test exception"));

        var handler = new RegisterCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _jwtProviderMock.Object);
        var command = new RegisterCommand("testuser", "TestPassword123!", "test@example.com");
        
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RegisterUserError");
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
