using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotesApp.Core.Models;
using NotesApp.Data.Repositories;
using Xunit;

namespace NotesApp.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public AuthIntegrationTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ShouldReturnToken_WhenPayloadIsValid()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "alice_1",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiSuccessPayload<AuthTokenPayload>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data!.Token));
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenUsernameAlreadyExists()
    {
        using var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "duplicate_user",
            password = "password123"
        });

        var second = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "duplicate_user",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var payload = await second.Content.ReadFromJsonAsync<ApiErrorPayload>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        using var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "bob_1",
            password = "password123"
        });

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "bob_1",
            password = "wrong_password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        var payload = await login.Content.ReadFromJsonAsync<ApiErrorPayload>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameFormatIsInvalid()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "bad user",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiValidationErrorPayload>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
        Assert.NotNull(payload.Errors);
        Assert.NotEmpty(payload.Errors!);
    }

    private sealed class ApiSuccessPayload<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class AuthTokenPayload
    {
        public string Token { get; set; } = string.Empty;
    }

    private sealed class ApiErrorPayload
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private sealed class ApiValidationErrorPayload
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }
    }
}

public class AuthApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserRepository>();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        });
    }
}

internal class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = [];
    private int _nextId = 1;
    private readonly object _lock = new();

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        lock (_lock)
        {
            var user = _users.SingleOrDefault(u => u.Username == username);
            return Task.FromResult(user);
        }
    }

    public Task<int> CreateUserAsync(User user)
    {
        lock (_lock)
        {
            user.Id = _nextId++;
            _users.Add(new User
            {
                Id = user.Id,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                CreatedAt = user.CreatedAt
            });

            return Task.FromResult(user.Id);
        }
    }
}
