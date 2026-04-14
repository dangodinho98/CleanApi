using CleanApi.Application.Abstractions;
using CleanApi.Application.Services;
using CleanApi.Domain.Entities;
using CleanApi.Shared.Builders;
using Moq;

namespace CleanApi.Application.Tests.UnitTests;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenIssuer> _jwt = new();

    private AuthService Sut => new(_users.Object, _hasher.Object, _jwt.Object);

    [Fact]
    public async Task RegisterAsyncThrowsWhenEmailInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.RegisterAsync("not-an-email", "password123", "Name"));
    }

    [Fact]
    public async Task RegisterAsyncThrowsWhenPasswordTooShort()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.RegisterAsync("a@test.com", "short", "Name"));
    }

    [Fact]
    public async Task RegisterAsyncThrowsWhenDisplayNameTooLong()
    {
        var name = new string('n', AuthService.MaxDisplayNameLength + 1);
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.RegisterAsync("a@test.com", "password123", name));
    }

    [Fact]
    public async Task RegisterAsyncThrowsWhenDisplayNameWhitespaceOnly() =>
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.RegisterAsync("a@test.com", "password123", "   "));

    [Fact]
    public async Task LoginAsyncThrowsWhenEmailInvalid() =>
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.LoginAsync("not-an-email", "password123"));

    [Fact]
    public async Task RegisterAsyncThrowsWhenDuplicateEmail()
    {
        _users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserBuilder.Default().WithEmail("a@test.com").Build());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Sut.RegisterAsync("A@Test.com", "password123", "Name"));
    }

    [Fact]
    public async Task RegisterAsyncInsertsHashedPasswordAndReturnsToken()
    {
        _users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _hasher.Setup(x => x.Hash("password123")).Returns("HASH");
        _jwt.Setup(x => x.CreateToken(It.IsAny<Guid>(), "a@test.com", "Name")).Returns("tok");

        User? inserted = null;
        _users.Setup(x => x.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => inserted = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var (user, token) = await Sut.RegisterAsync("a@test.com", "password123", "Name");

        Assert.Equal("tok", token);
        Assert.NotNull(inserted);
        Assert.Equal("HASH", inserted!.PasswordHash);
        Assert.Equal("a@test.com", user.Email);
        _users.Verify(x => x.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsyncThrowsWhenUserMissing()
    {
        _users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.LoginAsync("a@test.com", "password123"));
    }

    [Fact]
    public async Task LoginAsyncThrowsWhenPasswordInvalid()
    {
        var u = UserBuilder.Default()
            .WithEmail("a@test.com")
            .WithPasswordHash("HASH")
            .Build();
        _users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(u);
        _hasher.Setup(x => x.Verify("bad", "HASH")).Returns(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.LoginAsync("a@test.com", "bad"));
    }

    [Fact]
    public async Task GetUserByIdAsyncReturnsUserFromRepository()
    {
        var id = Guid.NewGuid();
        var u = UserBuilder.Default().WithId(id).Build();
        _users.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(u);

        var result = await Sut.GetUserByIdAsync(id);

        Assert.Same(u, result);
    }

    [Fact]
    public async Task LoginAsyncReturnsTokenWhenValid()
    {
        var u = UserBuilder.Default()
            .WithEmail("a@test.com")
            .WithPasswordHash("HASH")
            .WithDisplayName("N")
            .Build();
        _users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(u);
        _hasher.Setup(x => x.Verify("password123", "HASH")).Returns(true);
        _jwt.Setup(x => x.CreateToken(u.Id, u.Email, u.DisplayName)).Returns("jwt");

        var (user, token) = await Sut.LoginAsync("a@test.com", "password123");

        Assert.Equal("jwt", token);
        Assert.Equal(u.Id, user.Id);
    }
}
