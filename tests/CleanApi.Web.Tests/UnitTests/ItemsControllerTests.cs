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

public sealed class ItemsControllerTests
{
    private static ControllerContext ControllerContextWithUser(Guid userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            authenticationType: "Test");
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
    }

    [Fact]
    public async Task ListReturnsOkWithMappedRows()
    {
        var id = Guid.NewGuid();
        var at = DateTime.UtcNow;
        var rows = new List<Item>
        {
            new() { Id = id, Title = "A", Description = "B", CreatedAtUtc = at, OwnerUserId = null }
        };
        var repo = new Mock<IItemRepository>();
        repo.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        var sut = new ItemsController(new ItemService(repo.Object));

        var result = await sut.List(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<ItemResponse>>(ok.Value);
        Assert.Single(payload);
        Assert.Equal("A", payload[0].Title);
    }

    [Fact]
    public async Task GetReturnsNotFoundWhenMissing()
    {
        var repo = new Mock<IItemRepository>();
        repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Item?)null);
        var sut = new ItemsController(new ItemService(repo.Object));

        var result = await sut.Get(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateReturnsBadRequestWhenTitleInvalid()
    {
        var repo = new Mock<IItemRepository>();
        var sut = new ItemsController(new ItemService(repo.Object))
        {
            ControllerContext = ControllerContextWithUser(Guid.NewGuid())
        };

        var result = await sut.Create(new CreateItemRequest { Title = "  ", Description = "ok" }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        repo.Verify(x => x.InsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReturnsBadRequestWhenDescriptionWhitespaceOnly()
    {
        var repo = new Mock<IItemRepository>();
        var sut = new ItemsController(new ItemService(repo.Object))
        {
            ControllerContext = ControllerContextWithUser(Guid.NewGuid())
        };

        var result = await sut.Create(new CreateItemRequest { Title = "Title", Description = "   " }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        repo.Verify(x => x.InsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReturnsCreatedWhenValid()
    {
        var owner = Guid.NewGuid();
        var repo = new Mock<IItemRepository>();
        repo.Setup(x => x.InsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item i, CancellationToken _) => i);
        var sut = new ItemsController(new ItemService(repo.Object))
        {
            ControllerContext = ControllerContextWithUser(owner)
        };

        var result = await sut.Create(
            new CreateItemRequest { Title = "  Title ", Description = " Desc " },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ItemsController.Get), created.ActionName);
        var body = Assert.IsType<ItemResponse>(created.Value);
        Assert.Equal("Title", body.Title);
        Assert.Equal("Desc", body.Description);
        Assert.Equal(owner, body.OwnerUserId);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundWhenItemMissing()
    {
        var repo = new Mock<IItemRepository>();
        repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Item?)null);
        var sut = new ItemsController(new ItemService(repo.Object))
        {
            ControllerContext = ControllerContextWithUser(Guid.NewGuid())
        };

        var result = await sut.Update(
            Guid.NewGuid(),
            new UpdateItemRequest { Title = "a", Description = "b" },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteReturnsNotFoundWhenMissing()
    {
        var repo = new Mock<IItemRepository>();
        repo.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sut = new ItemsController(new ItemService(repo.Object));

        var result = await sut.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
