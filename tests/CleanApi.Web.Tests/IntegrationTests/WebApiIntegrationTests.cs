using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CleanApi.Shared;
using CleanApi.Web.Contracts;
using CleanApi.Web.Tests.IntegrationTests.Builders;

namespace CleanApi.Web.Tests.IntegrationTests;

[Collection(nameof(WebHostIntegrationFixture))]
public sealed class WebApiIntegrationTests(WebHostIntegrationFixture fixture)
{
    [SkippableFact]
    public async Task AccountMeRequiresAuthentication()
    {
        Skip.If(
            fixture.Skip,
            $"Set {TestConfigurationBuilder.SqlConnectionSettingKey} in appsettings.json (see tests/CleanApi.Web.Tests).");

        var api = ApiClientBuilder.For(fixture.Factory!);
        var res = await api.GetAccountMeAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [SkippableFact]
    public async Task RegisterReturnsBadRequestWithValidationErrorsWhenPasswordTooShort()
    {
        Skip.If(
            fixture.Skip,
            $"Set {TestConfigurationBuilder.SqlConnectionSettingKey} in appsettings.json (see tests/CleanApi.Web.Tests).");

        var api = ApiClientBuilder.For(fixture.Factory!);
        var res = await api.RegisterAsync("user@test.com", "short", "Name");

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("errors", out _) || doc.RootElement.TryGetProperty("Errors", out _));
    }

    [SkippableFact]
    public async Task AuthRegisterLoginCrudItemsFlow()
    {
        Skip.If(
            fixture.Skip,
            $"Set {TestConfigurationBuilder.SqlConnectionSettingKey} in appsettings.json (see tests/CleanApi.Web.Tests).");

        var api = ApiClientBuilder.For(fixture.Factory!);
        var email = $"user_{Guid.NewGuid():N}@test.com";

        var registerRes = await api.RegisterAsync(email, "password123", "Integration User");
        registerRes.EnsureSuccessStatusCode();
        var registerBody = await registerRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registerBody?.Token);

        var loginRes = await api.LoginAsync(email, "password123");
        loginRes.EnsureSuccessStatusCode();
        var loginBody = await loginRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginBody?.Token);

        api.SetBearerToken(loginBody!.Token);

        var meRes = await api.GetAccountMeAsync();
        meRes.EnsureSuccessStatusCode();
        var me = await meRes.Content.ReadFromJsonAsync<UserSummary>();
        Assert.Equal(email, me!.Email);

        var createRes = await api.CreateItemAsync("Hello", "World");
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var created = await createRes.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        var listRes = await api.ListItemsAsync();
        listRes.EnsureSuccessStatusCode();
        var list = await listRes.Content.ReadFromJsonAsync<List<ItemResponse>>();
        Assert.Contains(list!, x => x.Id == created!.Id);

        var getRes = await api.GetItemAsync(created.Id);
        getRes.EnsureSuccessStatusCode();

        var putRes = await api.UpdateItemAsync(created.Id, "Hello2", "World2");
        Assert.Equal(HttpStatusCode.NoContent, putRes.StatusCode);

        var deleteRes = await api.DeleteItemAsync(created.Id);
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var missing = await api.GetItemAsync(created.Id);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }
}
