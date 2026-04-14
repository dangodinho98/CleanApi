using CleanApi.Application.Abstractions;
using CleanApi.Application.Services;
using CleanApi.Domain.Entities;
using CleanApi.Web.Contracts;
using CleanApi.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CleanApi.Web.Tests.UnitTests;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task RegisterReturnsConflictWhenEmailExists()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "a@test.com", PasswordHash = "h", DisplayName = "X" });
        var sut = new AuthController(new AuthService(users.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenIssuer>()));

        var result = await sut.Register(
            new RegisterRequest { Email = "a@test.com", Password = "password123", DisplayName = "N" },
            CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task RegisterReturnsBadRequestOnArgumentException()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var sut = new AuthController(new AuthService(users.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenIssuer>()));

        var result = await sut.Register(
            new RegisterRequest { Email = "bad-email", Password = "password123", DisplayName = "N" },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task LoginReturnsUnauthorizedWhenInvalidCredentials()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var hasher = new Mock<IPasswordHasher>();
        var sut = new AuthController(new AuthService(users.Object, hasher.Object, Mock.Of<IJwtTokenIssuer>()));

        var result = await sut.Login(
            new LoginRequest { Email = "a@test.com", Password = "password123" },
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task LoginReturnsBadRequestWhenEmailInvalid()
    {
        var sut = new AuthController(new AuthService(Mock.Of<IUserRepository>(), Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenIssuer>()));

        var result = await sut.Login(
            new LoginRequest { Email = "not-an-email", Password = "password123" },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task LoginReturnsOkWithTokenWhenValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@test.com",
            PasswordHash = "HASH",
            DisplayName = "Me"
        };
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(x => x.Verify("password123", "HASH")).Returns(true);
        var jwt = new Mock<IJwtTokenIssuer>();
        jwt.Setup(x => x.CreateToken(user.Id, user.Email, user.DisplayName)).Returns("tok");
        var sut = new AuthController(new AuthService(users.Object, hasher.Object, jwt.Object));

        var result = await sut.Login(
            new LoginRequest { Email = "a@test.com", Password = "password123" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("tok", body.Token);
        Assert.Equal(user.Email, body.User.Email);
    }
}
