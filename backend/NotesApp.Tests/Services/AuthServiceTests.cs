using Microsoft.Extensions.Configuration;
using Moq;
using NotesApp.Core.DTOs;
using NotesApp.Core.Exceptions;
using NotesApp.Core.Models;
using NotesApp.Data.Repositories;
using NotesApp.Services;
using Xunit;

namespace NotesApp.Tests.Services;

public class AuthServiceTests
{
    private static IAuthService CreateService(Mock<IUserRepository> userRepository)
    {
        var settings = new Dictionary<string, string?>
        {
            ["JwtSettings:Key"] = "super_secret_jwt_key_that_is_32_bytes_minimum_for_tests_123",
            ["JwtSettings:Issuer"] = "notes_app_issuer",
            ["JwtSettings:Audience"] = "notes_app_audience",
            ["JwtSettings:DurationInMinutes"] = "60"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new AuthService(userRepository.Object, configuration);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowConflict_WhenUsernameAlreadyExists()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByUsernameAsync("existing_user"))
            .ReturnsAsync(new User { Id = 1, Username = "existing_user", PasswordHash = "hash", CreatedAt = DateTime.UtcNow });

        var service = CreateService(repo);
        var dto = new UserRegisterDto { Username = "existing_user", Password = "password123" };

        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(dto));
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnToken_WhenPayloadIsValid()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByUsernameAsync("new_user"))
            .ReturnsAsync((User?)null);
        repo.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(10);

        var service = CreateService(repo);
        var dto = new UserRegisterDto { Username = "new_user", Password = "password123" };

        var token = await service.RegisterAsync(dto);

        Assert.False(string.IsNullOrWhiteSpace(token));
        repo.Verify(r => r.CreateUserAsync(It.Is<User>(u => u.Username == "new_user")), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenCredentialsAreInvalid()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByUsernameAsync("john"))
            .ReturnsAsync(new User
            {
                Id = 12,
                Username = "john",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("right_password"),
                CreatedAt = DateTime.UtcNow
            });

        var service = CreateService(repo);
        var dto = new UserLoginDto { Username = "john", Password = "wrong_password" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByUsernameAsync("john"))
            .ReturnsAsync(new User
            {
                Id = 42,
                Username = "john",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("right_password"),
                CreatedAt = DateTime.UtcNow
            });

        var service = CreateService(repo);
        var dto = new UserLoginDto { Username = "john", Password = "right_password" };

        var token = await service.LoginAsync(dto);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowBadRequest_WhenUsernameIsWhitespace()
    {
        var repo = new Mock<IUserRepository>();
        var service = CreateService(repo);

        var dto = new UserLoginDto { Username = "   ", Password = "password123" };

        await Assert.ThrowsAsync<BadRequestException>(() => service.LoginAsync(dto));
    }
}
