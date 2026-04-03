using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Auth;

namespace TaskManagement.Infrastructure.Tests;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        _sut = new JwtTokenService(
            secret: "ThisIsAVeryLongSecretKeyForTestingPurposes1234567890!",
            issuer: "TestIssuer",
            audience: "TestAudience",
            expirationMinutes: 60
        );
    }

    [Fact]
    public void GenerateToken_ShouldReturnNonEmptyString()
    {
        var user = User.Create("testuser", "test@example.com", "hash");

        var token = _sut.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtFormat()
    {
        var user = User.Create("testuser", "test@example.com", "hash");

        var token = _sut.GenerateToken(user);

        // JWT has 3 parts separated by dots
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_DifferentUsers_ShouldReturnDifferentTokens()
    {
        var user1 = User.Create("user1", "user1@example.com", "hash");
        var user2 = User.Create("user2", "user2@example.com", "hash");

        var token1 = _sut.GenerateToken(user1);
        var token2 = _sut.GenerateToken(user2);

        token1.Should().NotBe(token2);
    }
}
