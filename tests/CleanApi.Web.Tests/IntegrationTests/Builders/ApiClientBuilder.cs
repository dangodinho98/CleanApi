using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanApi.Web.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CleanApi.Web.Tests.IntegrationTests.Builders;

public sealed class ApiClientBuilder
{
    private readonly HttpClient _client;

    private ApiClientBuilder(HttpClient client) => _client = client;

    public static ApiClientBuilder For(WebApplicationFactory<Program> factory) =>
        new(factory.CreateClient());

    public HttpClient Client => _client;

    public Task<HttpResponseMessage> RegisterAsync(string email, string password, string displayName) =>
        _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest { Email = email, Password = password, DisplayName = displayName });

    public Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });

    public void SetBearerToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public Task<HttpResponseMessage> GetAccountMeAsync() =>
        _client.GetAsync("/api/account/me");

    public Task<HttpResponseMessage> CreateItemAsync(string title, string description) =>
        _client.PostAsJsonAsync("/api/items", new CreateItemRequest { Title = title, Description = description });

    public Task<HttpResponseMessage> ListItemsAsync() => _client.GetAsync("/api/items");

    public Task<HttpResponseMessage> GetItemAsync(Guid id) => _client.GetAsync($"/api/items/{id}");

    public Task<HttpResponseMessage> UpdateItemAsync(Guid id, string title, string description) =>
        _client.PutAsJsonAsync($"/api/items/{id}", new UpdateItemRequest { Title = title, Description = description });

    public Task<HttpResponseMessage> DeleteItemAsync(Guid id) => _client.DeleteAsync($"/api/items/{id}");
}
