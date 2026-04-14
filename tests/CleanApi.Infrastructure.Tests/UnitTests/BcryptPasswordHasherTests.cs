using CleanApi.Infrastructure.Security;

namespace CleanApi.Infrastructure.Tests.UnitTests;

public sealed class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _sut = new();

    [Fact]
    public void HashThenVerifySucceeds()
    {
        var hash = _sut.Hash("my-secret-password");
        Assert.True(_sut.Verify("my-secret-password", hash));
    }

    [Fact]
    public void VerifyReturnsFalseForWrongPassword()
    {
        var hash = _sut.Hash("correct-horse-battery-staple");
        Assert.False(_sut.Verify("wrong", hash));
    }
}
