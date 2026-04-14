using CleanApi.Infrastructure.Tests.IntegrationTests.Builders;
using CleanApi.Shared;
using CleanApi.Shared.Builders;

namespace CleanApi.Infrastructure.Tests.IntegrationTests;

[Collection(nameof(SqlServerIntegrationFixture))]
public sealed class SqlServerRepositoryIntegrationTests(SqlServerIntegrationFixture fixture)
{
    [SkippableFact]
    public async Task ItemRepositoryRoundtrip()
    {
        Skip.If(
            fixture.Skip,
            $"Set {TestConfigurationBuilder.SqlConnectionSettingKey} in appsettings.json (see tests/CleanApi.Infrastructure.Tests).");

        var ctx = await new SqlIntegrationContextBuilder()
            .WithConnectionString(fixture.ConnectionString!)
            .BuildAsync();

        var user = UserBuilder.Default()
            .WithEmail($"u{Guid.NewGuid():N}@test.com")
            .WithPasswordHash("x")
            .WithDisplayName("Tester")
            .Build();
        await ctx.Users.InsertAsync(user);

        var byEmail = await ctx.Users.GetByEmailAsync(user.Email);
        Assert.NotNull(byEmail);
        Assert.Equal(user.Id, byEmail!.Id);
        Assert.Equal(user.Email, byEmail.Email);

        var item = await ctx.Items.InsertAsync(
            ItemBuilder.Default()
                .WithTitle("T")
                .WithDescription("D")
                .WithOwnerUserId(user.Id)
                .Build(),
            CancellationToken.None);

        var loaded = await ctx.Items.GetByIdAsync(item.Id);
        Assert.NotNull(loaded);
        Assert.Equal("T", loaded!.Title);
        Assert.Equal(user.Id, loaded.OwnerUserId);

        Assert.True(await ctx.Items.UpdateAsync(
            ItemBuilder.Default()
                .WithId(item.Id)
                .WithTitle("T2")
                .WithDescription("D2")
                .WithCreatedAtUtc(item.CreatedAtUtc)
                .WithOwnerUserId(user.Id)
                .Build()));

        var listed = await ctx.Items.ListAsync();
        Assert.Contains(listed, x => x.Id == item.Id);

        Assert.True(await ctx.Items.DeleteAsync(item.Id));
        Assert.Null(await ctx.Items.GetByIdAsync(item.Id));
    }
}
