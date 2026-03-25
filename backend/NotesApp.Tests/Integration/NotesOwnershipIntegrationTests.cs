using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Data.Repositories;
using NotesApp.Core.Models;
using Xunit;

namespace NotesApp.Tests.Integration;

public class NotesOwnershipIntegrationTests : IClassFixture<NotesApiFactory>
{
    private readonly NotesApiFactory _factory;

    public NotesOwnershipIntegrationTests(NotesApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenRequestingAnotherUsersNote()
    {
        using var client = CreateAuthorizedClient(userId: 1);

        var response = await client.GetAsync("/api/notes/2");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiErrorPayload>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUpdatingAnotherUsersNote()
    {
        using var client = CreateAuthorizedClient(userId: 1);

        var response = await client.PutAsJsonAsync("/api/notes/2", new
        {
            title = "changed",
            content = "updated"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDeletingAnotherUsersNote()
    {
        using var client = CreateAuthorizedClient(userId: 1);

        var response = await client.DeleteAsync("/api/notes/2");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOnlyCurrentUsersNotes()
    {
        using var client = CreateAuthorizedClient(userId: 1);

        var response = await client.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiSuccessPayload<List<NoteDtoPayload>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Single(payload.Data!);
        Assert.All(payload.Data!, note => Assert.Equal(1, note.UserId));
    }

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiErrorPayload>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
        Assert.Equal("Authentication is required.", payload.Message);
    }

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.value");

        var response = await client.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiErrorPayload>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
        Assert.Equal("Authentication is required.", payload.Message);
    }

    private HttpClient CreateAuthorizedClient(int userId)
    {
        var client = _factory.CreateClient();
        var token = _factory.CreateToken(userId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private sealed class ApiErrorPayload
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private sealed class ApiSuccessPayload<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class NoteDtoPayload
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}

public class NotesApiFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryNoteRepository _repo = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<INoteRepository>();
            services.AddSingleton<INoteRepository>(_repo);
        });
    }

    public string CreateToken(int userId)
    {
        using var scope = Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var jwt = configuration.GetSection("JwtSettings");

        var key = jwt["Key"]!;
        var issuer = jwt["Issuer"]!;
        var audience = jwt["Audience"]!;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"user_{userId}")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

internal class InMemoryNoteRepository : INoteRepository
{
    private readonly List<Note> _notes =
    [
        new Note
        {
            Id = 1,
            Title = "u1 note",
            Content = "owned by user 1",
            UserId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        },
        new Note
        {
            Id = 2,
            Title = "u2 note",
            Content = "owned by user 2",
            UserId = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        }
    ];

    public Task<IEnumerable<Note>> GetUserNotesAsync(int userId)
    {
        var notes = _notes.Where(n => n.UserId == userId).ToList();
        return Task.FromResult<IEnumerable<Note>>(notes);
    }

    public Task<Note?> GetNoteByIdAsync(int id, int userId)
    {
        var note = _notes.SingleOrDefault(n => n.Id == id && n.UserId == userId);
        return Task.FromResult(note);
    }

    public Task<int> CreateNoteAsync(Note note)
    {
        var newId = _notes.Max(n => n.Id) + 1;
        note.Id = newId;
        _notes.Add(note);
        return Task.FromResult(newId);
    }

    public Task<int> UpdateNoteAsync(Note note)
    {
        var existing = _notes.SingleOrDefault(n => n.Id == note.Id && n.UserId == note.UserId);
        if (existing == null)
            return Task.FromResult(0);

        existing.Title = note.Title;
        existing.Content = note.Content;
        existing.UpdatedAt = note.UpdatedAt;
        return Task.FromResult(1);
    }

    public Task<int> DeleteNoteAsync(int id, int userId)
    {
        var existing = _notes.SingleOrDefault(n => n.Id == id && n.UserId == userId);
        if (existing == null)
            return Task.FromResult(0);

        _notes.Remove(existing);
        return Task.FromResult(1);
    }
}
