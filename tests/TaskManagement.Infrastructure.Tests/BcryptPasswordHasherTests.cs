using FluentAssertions;
using TaskManagement.Infrastructure.Auth;

namespace TaskManagement.Infrastructure.Tests;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ShouldReturnBcryptHash()
    {
        var hash = _sut.Hash("TestPassword123!");

        hash.Should().StartWith("$2a$");
        hash.Should().NotBe("TestPassword123!");
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var hash = _sut.Hash("TestPassword123!");

        _sut.Verify("TestPassword123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ShouldReturnFalse()
    {
        var hash = _sut.Hash("TestPassword123!");

        _sut.Verify("WrongPassword!", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_SamePasswordTwice_ShouldProduceDifferentHashes()
    {
        var hash1 = _sut.Hash("TestPassword123!");
        var hash2 = _sut.Hash("TestPassword123!");

        hash1.Should().NotBe(hash2);
    }
}
