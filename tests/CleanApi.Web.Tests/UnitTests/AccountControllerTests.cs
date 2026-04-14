using CleanApi.Application.Abstractions;
using CleanApi.Application.Services;
using CleanApi.Domain.Entities;
using CleanApi.Web.Contracts;
using CleanApi.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CleanApi.Web.Tests.UnitTests;

public sealed class AccountControllerTests
{
    [Fact]
    public async Task MeReturnsUnauthorizedWhenNameIdentifierMissing()
    {
        var sut = new AccountController(new AuthService(Mock.Of<IUserRepository>(), Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenIssuer>()))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await sut.Me(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task MeReturnsUnauthorizedWhenUserNotFound()
    {
        var id = Guid.NewGuid();
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var sut = new AccountController(new AuthService(users.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenIssuer>()))
        {
            ControllerContext = ControllerContextWithSub(id.ToString())
        };

        var result = await sut.Me(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task MeReturnsOkWhenUserExists()
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, Email = "x@test.com", DisplayName = "X" };
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var sut = new AccountController(new AuthService(users.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenIssuer>()))
        {
            ControllerContext = ControllerContextWithSub(id.ToString())
        };

        var result = await sut.Me(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<UserSummary>(ok.Value);
        Assert.Equal(id, body.Id);
        Assert.Equal("x@test.com", body.Email);
    }

    private static ControllerContext ControllerContextWithSub(string sub)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, sub)], authenticationType: "Test");
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
    }
}
