using Dapper;
using NotesApp.Core.Models;

namespace NotesApp.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<int> CreateUserAsync(User user);
}

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var query = @"
            SELECT Id, Username, PasswordHash, CreatedAt
            FROM Users
            WHERE Username = @Username";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username.Trim() });
    }

    public async Task<int> CreateUserAsync(User user)
    {
        var query = @"
            INSERT INTO Users (Username, PasswordHash, CreatedAt)
            VALUES (@Username, @PasswordHash, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, user);
    }
}
