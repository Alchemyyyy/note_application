using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Core.DTOs;
using NotesApp.Core.Exceptions;
using NotesApp.Core.Models;
using NotesApp.Data.Repositories;

namespace NotesApp.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly double _jwtDurationInMinutes;
    private readonly SigningCredentials _signingCredentials;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;

        var jwtSettings = configuration.GetSection("JwtSettings");
        var keyValue = jwtSettings["Key"] ?? throw new InvalidOperationException("JwtSettings:Key is missing.");
        _jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is missing.");
        _jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is missing.");
        var duration = jwtSettings["DurationInMinutes"] ?? throw new InvalidOperationException("JwtSettings:DurationInMinutes is missing.");

        if (!double.TryParse(duration, out _jwtDurationInMinutes) || _jwtDurationInMinutes <= 0)
            throw new InvalidOperationException("JwtSettings:DurationInMinutes must be a positive number.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public async Task<string> RegisterAsync(UserRegisterDto dto)
    {
        var username = dto.Username.Trim();
        if (string.IsNullOrWhiteSpace(username))
            throw new BadRequestException("Username is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new BadRequestException("Password is required.");

        var existingUser = await _userRepository.GetUserByUsernameAsync(username);
        if (existingUser != null)
            throw new ConflictException("Username already exists.");

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        var userId = await _userRepository.CreateUserAsync(user);
        user.Id = userId;
        
        return GenerateJwtToken(user);
    }

    public async Task<string> LoginAsync(UserLoginDto dto)
    {
        var username = dto.Username.Trim();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(dto.Password))
            throw new BadRequestException("Username and password are required.");

        var user = await _userRepository.GetUserByUsernameAsync(username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid username or password.");

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtDurationInMinutes),
            signingCredentials: _signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
