using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.WebAPI.Controllers;

namespace TaskManagement.WebAPI.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _sut = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnCreated()
    {
        var request = new RegisterRequest { Username = "newuser", Email = "new@test.com", Password = "Pass123!" };
        var response = new UserResponse { Id = Guid.NewGuid(), Username = "newuser", Email = "new@test.com", CreatedAt = DateTime.UtcNow };
        _authServiceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync(response);

        var result = await _sut.Register(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        var request = new LoginRequest { Username = "admin", Password = "Admin123!" };
        var response = new LoginResponse { Token = "jwt-token", Username = "admin", Email = "admin@test.com", UserId = Guid.NewGuid() };
        _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(response);

        var result = await _sut.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(response);
    }
}
