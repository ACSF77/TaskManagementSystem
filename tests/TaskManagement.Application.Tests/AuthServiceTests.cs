using FluentAssertions;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _jwtMock = new Mock<IJwtTokenService>();
        _sut = new AuthService(_userRepoMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.Hash("Password1!")).Returns("hashed");

        var request = new RegisterRequest { Username = "newuser", Email = "new@test.com", Password = "Password1!" };

        var result = await _sut.RegisterAsync(request);

        result.Username.Should().Be("newuser");
        result.Email.Should().Be("new@test.com");
        result.Id.Should().NotBeEmpty();
        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldThrowConflict()
    {
        var existing = User.Create("taken", "other@test.com", "hash");
        _userRepoMock.Setup(r => r.GetByUsernameAsync("taken")).ReturnsAsync(existing);

        var request = new RegisterRequest { Username = "taken", Email = "new@test.com", Password = "Password1!" };

        var act = () => _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*Username*");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowConflict()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var existing = User.Create("other", "taken@test.com", "hash");
        _userRepoMock.Setup(r => r.GetByEmailAsync("taken@test.com")).ReturnsAsync(existing);

        var request = new RegisterRequest { Username = "newuser", Email = "taken@test.com", Password = "Password1!" };

        var act = () => _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*Email*");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        var user = User.Create("admin", "admin@test.com", "hashedpw");
        _userRepoMock.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("Password1!", "hashedpw")).Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns("jwt-token-123");

        var request = new LoginRequest { Username = "admin", Password = "Password1!" };

        var result = await _sut.LoginAsync(request);

        result.Token.Should().Be("jwt-token-123");
        result.Username.Should().Be("admin");
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ShouldThrowUnauthorized()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("nobody")).ReturnsAsync((User?)null);

        var request = new LoginRequest { Username = "nobody", Password = "Password1!" };

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowUnauthorized()
    {
        var user = User.Create("admin", "admin@test.com", "hashedpw");
        _userRepoMock.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("wrongpw", "hashedpw")).Returns(false);

        var request = new LoginRequest { Username = "admin", Password = "wrongpw" };

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
