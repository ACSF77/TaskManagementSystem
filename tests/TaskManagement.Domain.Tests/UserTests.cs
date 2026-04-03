using FluentAssertions;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        var user = User.Create("testuser", "test@example.com", "hashedpassword");

        user.Id.Should().NotBeEmpty();
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hashedpassword");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldTrimUsernameAndLowercaseEmail()
    {
        var user = User.Create("  TestUser  ", "  Test@Example.COM  ", "hash");

        user.Username.Should().Be("TestUser");
        user.Email.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUsername_ShouldThrow(string? username)
    {
        var act = () => User.Create(username!, "test@example.com", "hash");
        act.Should().Throw<ArgumentException>().WithParameterName("username");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ShouldThrow(string? email)
    {
        var act = () => User.Create("user", email!, "hash");
        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void Create_WithEmailMissingAtSign_ShouldThrow()
    {
        var act = () => User.Create("user", "invalidemail", "hash");
        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPasswordHash_ShouldThrow(string? hash)
    {
        var act = () => User.Create("user", "test@example.com", hash!);
        act.Should().Throw<ArgumentException>().WithParameterName("passwordHash");
    }

    [Fact]
    public void FromStorage_ShouldRestoreAllProperties()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var user = User.FromStorage(id, "admin", "admin@test.com", "hash123", createdAt);

        user.Id.Should().Be(id);
        user.Username.Should().Be("admin");
        user.Email.Should().Be("admin@test.com");
        user.PasswordHash.Should().Be("hash123");
        user.CreatedAt.Should().Be(createdAt);
    }
}
