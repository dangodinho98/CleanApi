using CleanApi.Application.Abstractions;
using CleanApi.Application.Services;
using CleanApi.Domain.Entities;
using CleanApi.Shared.Builders;
using Moq;

namespace CleanApi.Application.Tests.UnitTests;

public sealed class ItemServiceTests
{
    private readonly Mock<IItemRepository> _items = new();
    private ItemService Sut => new(_items.Object);

    [Fact]
    public async Task CreateAsync_throws_when_title_empty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.CreateAsync("  ", "d", null));
    }

    [Fact]
    public async Task CreateAsync_throws_when_description_whitespace_only() =>
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.CreateAsync("t", "   ", null));

    [Fact]
    public async Task CreateAsync_throws_when_description_too_long()
    {
        var longDesc = new string('x', ItemService.MaxDescriptionLength + 1);
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.CreateAsync("t", longDesc, null));
    }

    [Fact]
    public async Task CreateAsync_throws_when_title_exceeds_max_length()
    {
        var title = new string('x', ItemService.MaxTitleLength + 1);
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.CreateAsync(title, "ok", null));
    }

    [Fact]
    public async Task UpdateAsync_throws_when_title_empty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.UpdateAsync(Guid.NewGuid(), "  ", "d"));
    }

    [Fact]
    public async Task UpdateAsync_throws_when_description_whitespace_only()
    {
        var id = Guid.NewGuid();
        var existing = ItemBuilder.Default()
            .WithId(id)
            .WithTitle("a")
            .WithDescription("b")
            .Build();
        _items.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await Assert.ThrowsAsync<ArgumentException>(() => Sut.UpdateAsync(id, "t", "   "));
    }

    [Fact]
    public async Task CreateAsync_inserts_trimmed_fields()
    {
        Item? captured = null;
        _items.Setup(x => x.InsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .Callback<Item, CancellationToken>((i, _) => captured = i)
            .ReturnsAsync((Item i, CancellationToken _) => i);

        var owner = Guid.NewGuid();
        var result = await Sut.CreateAsync("  Hello ", "  world ", owner);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Hello", result.Title);
        Assert.Equal("world", result.Description);
        Assert.Equal(owner, result.OwnerUserId);
        _items.Verify(x => x.InsertAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_returns_false_when_missing()
    {
        _items.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

        var ok = await Sut.UpdateAsync(Guid.NewGuid(), "a", "b");

        Assert.False(ok);
        _items.Verify(x => x.UpdateAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_updates_when_found()
    {
        var id = Guid.NewGuid();
        var existing = ItemBuilder.Default()
            .WithId(id)
            .WithTitle("old")
            .WithDescription("oldd")
            .Build();
        _items.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _items.Setup(x => x.UpdateAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ok = await Sut.UpdateAsync(id, "n", "nn");

        Assert.True(ok);
        _items.Verify(x => x.UpdateAsync(It.Is<Item>(i => i.Title == "n" && i.Description == "nn"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
